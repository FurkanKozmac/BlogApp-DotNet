# BlogApp — Junior Backend Mülakat Hazırlık Raporu

## İnceleme kapsamı

Rapor, mevcut C# kaynakları, `.csproj` dosyaları, testler ve CI YAML dosyasının statik incelemesine dayanır. README'deki açıklamalar tek başına kanıt sayılmadı. “Tasarım gerekçesi” kod düzeninden çıkarılabiliyorsa çıkarım olarak işaretlendi; test edilmemiş çalışma zamanı davranışları kesinmiş gibi sunulmadı.

## Genel mimari

### Dört katman ve proje bağımlılıkları

Üretim mimarisindeki dört proje/katman:

| Katman | Sorumluluk | Proje referansları |
|---|---|---|
| API | HTTP controller'ları, auth middleware/konfigürasyonu, composition root | Application, Infrastructure |
| Application | Servis/repository arayüzleri, request modelleri, DTO'lar ve FluentValidation validator'ları | Domain |
| Domain | Blog entity'leri ve ortak entity alanları | Başka proje referansı yok |
| Infrastructure | Servis ve repository implementasyonları, Identity, EF Core/SQL Server, dosya servisi | Application, Domain |

Bu yönler `.csproj` dosyalarındaki `ProjectReference` kayıtlarından çıkarılmıştır. Test projesi bu dört üretim katmanından biri değildir.

```text
API ───────────────> Application ─────> Domain
 │                         ▲
 └──────> Infrastructure ──┴─────────> Domain
```

API'nin Infrastructure'ı referans etmesi DI composition root'unun burada kurulmasıyla açıklanabilir; bu gerekçe kod düzeninden yapılan çıkarımdır, repoda ADR yoktur. Servis arayüzleri Application'da, implementasyonları Infrastructure'dadır. Buna rağmen `CreatePostRequest` içinde `IFormFile` bulunması Application katmanını ASP.NET Core HTTP tipine bağlar; katman ayrımı tamamen framework bağımsız değildir.

Kanıt: [BlogApp.API.csproj](./BlogApp.API/BlogApp.API.csproj), [BlogApp.Application.csproj](./BlogApp.Application/BlogApp.Application.csproj), [BlogApp.Domain.csproj](./BlogApp.Domain/BlogApp.Domain.csproj), [BlogApp.Infrastructure.csproj](./BlogApp.Infrastructure/BlogApp.Infrastructure.csproj), [Program.cs](./BlogApp.API/Program.cs), [ApplicationServiceRegistration.cs](./BlogApp.Application/ApplicationServiceRegistration.cs), [InfrastructureServiceRegistration.cs](./BlogApp.Infrastructure/InfrastructureServiceRegistration.cs).

### İstek akışı

Controller, Application'daki servis arayüzünü çağırır; Infrastructure implementasyonu request doğrular, repository arayüzü/Identity/dosya servisiyle çalışır; EF repository'leri `BlogDbContext` üzerinden SQL Server'a gider. MediatR command/query/handler ve pipeline behavior mevcut kodda yoktur.

Bu, controller'ları HTTP bağlama/yanıt işlerine, servisleri kullanım senaryolarına, repository'leri veri erişimine ayırır. Bu tercih servisleri unit test etmeyi kolaylaştırabilir; bu son ifade tasarım gerekçesi/çıkarımdır.

## Ana özellikler

### Kimlik doğrulama, kayıt ve roller

- `POST /api/auth/register`, `IAuthService.RegisterAsync` çağırır. `AuthService`, validator çalıştırır, `UserManager.CreateAsync` ile `AppUser` ekler ve `"User"` rolüne eklemeyi dener.
- `POST /api/auth/login`, kullanıcı adını Identity ile arar ve parolayı doğrular. Kullanıcı yoksa veya parola yanlışsa `InvalidCredentialsException` fırlatılır; middleware bunu 401'e map eder.
- JWT; user id, email, full name, `UserName` ve role claim'leri içerir. HS256 imzalanır. `ExpiryMinutes` kullanılarak UTC zamanında son kullanma değeri hesaplanır.
- Identity parola politikası minimum 6 karakterdir; digit, uppercase ve non-alphanumeric gereklilikleri kapatılmıştır.
- Roller `BlogDbContext` içinde `Admin` ve `User` olarak seed edilmiştir.

İlgili kaynaklar: [AuthController.cs](./BlogApp.API/Controllers/AuthController.cs), [IAuthService.cs](./BlogApp.Application/Interfaces/Services/IAuthService.cs), [AuthService.cs](./BlogApp.Infrastructure/Services/AuthService.cs), [RegisterRequestValidator.cs](./BlogApp.Application/Validators/RegisterRequestValidator.cs), [LoginRequestValidator.cs](./BlogApp.Application/Validators/LoginRequestValidator.cs), [InfrastructureServiceRegistration.cs](./BlogApp.Infrastructure/InfrastructureServiceRegistration.cs), [BlogDbContext.cs](./BlogApp.Infrastructure/Persistence/BlogDbContext.cs), [appsettings.json](./BlogApp.API/appsettings.json).

**Tasarım gerekçesi:** `UserManager` kullanımı Identity parola hashleme ve kullanıcı yönetimi API'sini uygulama koduna bırakır; ham parola DB'ye yazılmaz. Bu çerçeve davranışından çıkarımdır.

**Bilinen eksikler / dikkat noktaları**

- Refresh-token akışı, token yenileme veya revoke mekanizması görünmüyor.
- Kayıt işleminde kullanıcı oluşturulduktan sonra rol ekleme başarısız olursa servis validation exception döndürür; kullanıcı kaydıyla rol eklemeyi kapsayan transaction/compensation görünmüyor.
- Login başarısızlıkları aynı 401 mesajını döndürür; bu kullanıcı enumeration riskini azaltan tercih olarak yorumlanabilir.
- `appsettings.json` içindeki mevcut JWT secret alanı boş. Secret güvenilir config kaynağından gelmezse Infrastructure DI kaydı istisna fırlatır.
- Tarihçede secret alanı boş olmayan değerle yer almış: `2b2a642339ba8ea7907ab38c7540d0a15dfeae9b` ve `7bcb14e8019a75c65570bf79cd4e380fd931acc` commit’lerinde. Değer bu raporda gösterilmemiştir. Bu geçmiş kontrolü gizli değeri çıkarmadan `git log -p -- '*appsettings*'` patch’lerini taramıştır. Tarihçe değiştirilmedi; geçmişte kullanılan secret'ı döndürmek/rotate etmek operasyonel olarak değerlendirilmelidir.

### Post'lar

- **Listeleme:** `GET /api/posts` anonimdir. Page/pageSize validator'dan geçer; repository ayrı `CountAsync` çalıştırır ve sıralanmış `IQueryable` üzerinde `Skip/Take` ile sayfayı DB'den alır.
- **Detay:** `GET /api/posts/{id}` postu yorumlarıyla yükleyip `PostDetailDto` döndürür.
- **Oluşturma:** `POST /api/posts`, multipart request alabilir; servis başlık/içerik validasyonu yapar, görseli opsiyonel kaydeder, user id ve username'i geçerli kullanıcı bağlamından alır.
- **Güncelleme:** `PUT /api/posts`, servis postu bulur, sahibi veya Admin değilse `ForbiddenException` fırlatır; bu 403 olur.
- **Silme:** `DELETE /api/posts/{id}`, sahibi/Admin kontrolünden sonra `IsDeleted=true` işaretler; controller 204 döndürür.

İlgili kaynaklar: [PostsController.cs](./BlogApp.API/Controllers/PostsController.cs), [IPostService.cs](./BlogApp.Application/Interfaces/Services/IPostService.cs), [BlogPostService.cs](./BlogApp.Infrastructure/Services/BlogPostService.cs), [PostRepository.cs](./BlogApp.Infrastructure/Repositories/PostRepository.cs), [Post.cs](./BlogApp.Domain/Entities/Post.cs), [PostDto.cs](./BlogApp.Application/DTOs/PostDto.cs), [PostDetailDto.cs](./BlogApp.Application/DTOs/PostDetailDto.cs).

**Tasarım gerekçesi:** Post sahibi denetiminin serviste yapılması, controller katmanını kullanım senaryosu kuralından ayırır ve servis testlerinde gerçek kuralı doğrulamaya imkân verir. Bu, koddaki yerleşimden çıkarımdır.

**Bilinen eksikler / dikkat noktaları**

- Post bulunamadığında servis genel `Exception` fırlatıyor. Middleware bunu 500 yapıyor; entity için açık 404 mapping'i yok.
- Update işlemi 200 döndürürken delete 204 döndürür; update için 204 tercih edilmemiş.
- Sayfa sayısı üst sınırı `1_000_000`, page size üst sınırı `100`; bu değerler test edilir ama ürün gereksinimi olarak gerekçeleri repoda açıklanmıyor.
- `CountAsync` ve sayfa sorgusu ayrı SQL sorgularıdır; eşzamanlı veri değişiminde count ve page sonuçları farklı snapshot'ları temsil edebilir (transaction/isolation burada özel ayarlanmıyor).
- Servis cancellation token kabul etse de repository metotları token parametresi almıyor; DB çağrılarına request cancellation aktarılmıyor.

### Yorumlar

`POST /api/comments` `[Authorize]` altındadır. `CreateCommentRequest` yalnızca `PostId` ve `Text` taşır; service author alanını `ICurrentUserService.UserName` değerinden alır ve yorum DTO'su döndürür. `ICurrentUserService` API tarafında JWT `UserName` claim'ini, yoksa Identity.Name'i okur.

Kaynaklar: [CommentsController.cs](./BlogApp.API/Controllers/CommentsController.cs), [ICommentService.cs](./BlogApp.Application/Interfaces/Services/ICommentService.cs), [CommentService.cs](./BlogApp.Infrastructure/Services/CommentService.cs), [CreateCommentRequest.cs](./BlogApp.Application/Models/Requests/CreateCommentRequest.cs), [CurrentUserService.cs](./BlogApp.API/Services/CurrentUserService.cs), [CreateCommentRequestValidator.cs](./BlogApp.Application/Validators/CreateCommentRequestValidator.cs).

**Bilinen eksikler:** Comment validator `PostId > 0` ve metin boş değil kuralını uygular; servis postun gerçekten var olduğunu önceden kontrol etmiyor. İlişkisel bütünlük varsa nihai hata DB'den gelebilir; bu davranış canlı DB'de doğrulanmadı. `UserName` claim'i bulunamazsa servis `"Unknown"` fallback'i kullanır.

### Kategoriler

`POST /api/categories`, controller seviyesinde `[Authorize(Roles = "Admin")]` ile korunur. `CategoryService` kategori adı validator'ını çalıştırır ve generic repository ile ekler; başarıda controller 201 döndürür.

Kaynaklar: [CategoriesController.cs](./BlogApp.API/Controllers/CategoriesController.cs), [ICategoryService.cs](./BlogApp.Application/Interfaces/Services/ICategoryService.cs), [CategoryService.cs](./BlogApp.Infrastructure/Services/CategoryService.cs), [CreateCategoryRequestValidator.cs](./BlogApp.Application/Validators/CreateCategoryRequestValidator.cs), [Category.cs](./BlogApp.Domain/Entities/Category.cs).

**Bilinen eksikler:** Validator boş adı ve 100 karakter üstünü reddeder; kategori adı uniqueness kontrolü uygulama kodunda görünmüyor.

### FluentValidation ve hata yönetimi

Validator'lar Application assembly taramasıyla kaydedilir. MediatR pipeline yoktur; her servis kendi `IValidator<T>` örneğini alıp `ValidateAndThrowAsync` çağırır. FluentValidation exception'ı middleware'de 400'e dönüşür. `[ApiController]` model-binding/model-validation hataları da MVC'nin otomatik 400 davranışına tabidir.

Kaynaklar: [ApplicationServiceRegistration.cs](./BlogApp.Application/ApplicationServiceRegistration.cs), [RequestValidationExtensions.cs](./BlogApp.Infrastructure/Services/RequestValidationExtensions.cs), [ExceptionMiddleware.cs](./BlogApp.API/Middleware/ExceptionMiddleware.cs).

Middleware genel exception'ları da yakalar ve 500 döndürür. Genel hata mesajı response'a yazılır; burada logger kullanımı görünmüyor. Dolayısıyla post-not-found gibi beklenmeyen/genel exception'larda 404 yerine 500 oluşabilir ve iç exception mesajı istemciye çıkabilir.

### Soft delete ve tarih alanları

`Post`, `Comment`, `Category` için global `!IsDeleted` query filter vardır. `DeleteAsync` postu fiziksel kaldırmaz; `IsDeleted` değerini değiştirip repository ile kaydeder. `BlogDbContext.SaveChangesAsync`, `BaseEntity` türevlerinde create/update tarih alanlarını UTC ile ayarlar.

Kaynaklar: [BlogDbContext.cs](./BlogApp.Infrastructure/Persistence/BlogDbContext.cs), [BaseEntity.cs](./BlogApp.Domain/Common/BaseEntity.cs), [BlogPostService.cs](./BlogApp.Infrastructure/Services/BlogPostService.cs), [GenericRepository.cs](./BlogApp.Infrastructure/Repositories/GenericRepository.cs).

**Tasarım gerekçesi:** Soft delete, kayıtları fiziksel olarak kaldırmadan normal sorgulardan gizlemeyi sağlar. Bu gerekçe global query filter + `IsDeleted` kullanımından çıkarılmıştır.

**Bilinen eksikler:** Normal geri yükleme/admin geçmişi için ayrı servis yolu yok. `SaveChanges` senkron override edilmemiş; audit alanı güncellemesi async kaydetme yoluna bağlı.

### Görsel dosya yükleme

Post create sırasında opsiyonel `IFormFile`, `FileService` tarafından `wwwroot/uploads` klasörüne GUID dosya adıyla yazılır; dönen relative URL postta saklanır. API `UseStaticFiles()` çağırır.

Kaynaklar: [CreatePostRequest.cs](./BlogApp.Application/Models/Requests/CreatePostRequest.cs), [BlogPostService.cs](./BlogApp.Infrastructure/Services/BlogPostService.cs), [FileService.cs](./BlogApp.Infrastructure/Services/FileService.cs), [Program.cs](./BlogApp.API/Program.cs).

**Bilinen eksikler:** Dosya boyutu, MIME/content doğrulama veya uzantı allowlist'i görünmüyor. Dosya sistemi ve DB kaydını tek işlemde atomik hale getiren rollback/temizleme akışı da görünmüyor.

### Persistence ve repository'ler

`BlogDbContext` Identity DbContext'inden türemiştir. Generic repository CRUD ve save işlemlerini, `PostRepository` ise post detay/listesi için include ve sayfalama sorgularını yürütür. Bu çözümde açık bir Unit of Work interface'i görünmüyor; repository metotları kendi `SaveChangesAsync` çağrılarını yapıyor.

Kaynaklar: [BlogDbContext.cs](./BlogApp.Infrastructure/Persistence/BlogDbContext.cs), [GenericRepository.cs](./BlogApp.Infrastructure/Repositories/GenericRepository.cs), [PostRepository.cs](./BlogApp.Infrastructure/Repositories/PostRepository.cs), [InfrastructureServiceRegistration.cs](./BlogApp.Infrastructure/InfrastructureServiceRegistration.cs).

## Zor mülakat soruları ve cevapları

### 1. Dört katmanlı mimaride bağımlılık yönü nedir? Domain neden alt tarafta?

**Cevap:** `.csproj` referanslarına göre API Application ve Infrastructure'ı, Application Domain'i, Infrastructure Application ve Domain'i referanslıyor. Domain başka projeye referans vermiyor. Böylece entity'ler web/EF implementation detaylarından bağımsız duruyor. Ancak Application `IFormFile` kullandığı için framework bağımsızlığı tam değil.

### 2. MediatR kaldırılınca CQRS kayboldu mu?

**Cevap:** Ayrı command/query sınıfları ve dispatcher artık yok. Controller doğrudan `IPostService` gibi interface'leri çağırıyor. Read/write use case'leri serviste farklı metotlar olarak kalabilir; bu kendiliğinden ayrı read/write store veya tam CQRS mimarisi oluşturmaz.

### 3. Servis interface'leri Application'da, implementasyonları Infrastructure'da olmasının getirisi nedir?

**Cevap:** Controller compile-time'da servis sözleşmesine bağımlı olur; veri erişim/Identity implementasyonları servis arkasında kalır. Servis kuralları fake repository/current-user ile test edilebilir. Buna rağmen API, DI kaydı için Infrastructure'ı doğrudan referanslıyor.

### 4. Validator'lar hangi katmanda çalışıyor ve başarısızlık nasıl HTTP'ye dönüşüyor?

**Cevap:** Validator sınıfları Application'dadır, servisler `IValidator<T>` alıp çalıştırır, `RequestValidationExtensions` invalid sonuçta FluentValidation exception fırlatır, `ExceptionMiddleware` bunu 400'e map eder. MediatR pipeline kullanılmıyor. ASP.NET Core `[ApiController]` request model binding hataları da otomatik 400 üretebilir.

### 5. Pagination gerçekten DB tarafında mı?

**Cevap:** Evet. `PostRepository.GetPagedWithCategoryAsync`, EF `IQueryable` üzerinde önce `CountAsync`, sonra ordering, `Skip`, `Take`, `ToListAsync` çağırıyor. İlgili kaynak [PostRepository.cs](./BlogApp.Infrastructure/Repositories/PostRepository.cs) satır 25–39. Sayım ve page fetch ayrı sorgular.

### 6. `pageSize` limitini validator'da doğrulamak tek başına yeterli mi?

**Cevap:** Mevcut servis listeleme öncesi validator çalıştırıyor, dolayısıyla normal servis akışı repository'ye geçmeden invalid sayfayı reddeder. Bununla birlikte repository metodu doğrudan başka çağrılarda kullanılabilir; kritik veri erişim sınırlarında parametre önkoşullarını korumak veya validator'ın yalnız servis üzerinden erişileceğini netleştirmek düşünülebilir. Şu an repository ayrıca limit kontrolü yapmıyor.

### 7. Admin başka kullanıcının postunu nasıl güncelleyebiliyor?

**Cevap:** Servis `post.AppUserId != currentUser.UserId && !currentUser.IsAdmin` koşulunda forbidden exception üretir. Owner olmayan ve Admin olmayan kullanıcı 403 alır; Admin'in `IsAdmin` değeri true olduğu için güncelleme devam eder. Bu iki kural `BlogPostServiceTests` içinde servis seviyesinde testlidir.

### 8. Hatalı parola neden 401; validator hatası neden 400?

**Cevap:** Hatalı user/pass `InvalidCredentialsException` olarak temsil edilir ve middleware 401'e map eder. Eksik alan gibi request-validasyon hataları `ValidationException`'dır ve 400'e gider. Böylece beklenen kimlik bilgisi reddi genel 500 exception'ından ayrılır.

### 9. Post bulunamazsa hangi HTTP kodu dönüyor? 404 var mı?

**Cevap:** Mevcut servis `Exception("Post not found.")` fırlatıyor; middleware yalnız validation, credentials ve forbidden tiplerini özel eşler, kalanını 500 yapar. Dolayısıyla entity bulunamaması için açık 404 yok. URL route eşleşmemesi farklı olarak framework routing tarafından ele alınır; bu rapor canlı HTTP route testi değildir.

### 10. JWT claim'leri ve doğrulamaları nelerdir? Rol değişikliği token'a nasıl yansır?

**Cevap:** Token user id, email, full name, username ve role claim'lerini içerir. API issuer, audience, lifetime ve signing key'i doğrular; süre config'te 60 dakika. Rol claim'leri token üretilirken gömülür; mevcut token, rol güncellemesini otomatik olarak yeniden okuyacak biçimde tasarlanmamıştır.

### 11. Soft-delete global filter'ın bakım/operasyon tuzağı nedir?

**Cevap:** Normal sorgular silinmiş kaydı gizler; geri yükleme/denetim sorgusu için filter'ı aşan yetkili bir yol gerekir. Üç entity'de filter var. Ayrıca bu behavior'ı kapsayan test veya admin recovery API'si bu kapsamda görünmüyor.

### 12. Yorum yazarının JWT'den geldiğini nasıl doğrularsınız?

**Cevap:** Controller `[Authorize]` ile korumalı; request modelinde Author alanı yok; CommentService `ICurrentUserService.UserName` üzerinden atama yapıyor; API implementasyonu önce `"UserName"` claim'ini okuyor. `CreateComment_WhenRequestHasNoAuthor_UsesAuthenticatedUserName` bu servis sözleşmesi davranışını test ediyor. Bu test JWT imza doğrulamasını uçtan uca test etmez.

### 13. Dosya yazımı ve post kaydı arasında hata olursa ne olur?

**Cevap:** Dosya önce FileService ile diske yazılır, ardından repository postu kaydeder. DB kaydı başarısız olursa dosya silme/compensation gösterilmemiştir; yetim dosya kalabilir. Ters sırada da URL'si bozuk kayıt oluşma riski vardır. Bu iki sistemi kapsayan transaction burada yok.

### 14. AutoMapper neden kaldırıldı; alternatifin tradeoff'u ne?

**Cevap:** Tarama AutoMapper 13.0.1'i yüksek önem dereceli advisory ile gösterdi. NuGet aracı 15.1.3 önerdi; deneme güncellemesi mevcut testte `MapperConfiguration` constructor uyumsuzluğu çıkardı. Projedeki mapping sayısı az olduğu için bağımlılık kaldırıldı ve service içinde açık mapping yapıldı. Bu uyarıyı kaldırır ve mapping'i görünür kılar; karşılığında alan sayısı arttığında manuel mapping bakımı gerekir.

### 15. Testlerin sınırı nedir?

**Cevap:** xUnit testleri servisleri fake repository/identity ile doğruluyor ve middleware'in exception-status mapping'ini test ediyor. Gerçek SQL Server, JWT imza üretme/doğrulama pipeline'ı veya HTTP server end-to-end testi bu test projesinde görünmüyor.

## CV doğrulama

### Katman sayısı ve referanslar

**Üretim katmanı: 4.** API → Application + Infrastructure; Application → Domain; Infrastructure → Application + Domain; Domain → proje referansı yok. Kanıt dört `.csproj` dosyasıdır. Test projesi ayrı beşinci solution projesidir ve üretim katmanına dahil değildir.

### Servis arayüzleri ve implementasyonları

| Arayüz | Implementasyon | Konum |
|---|---|---|
| `IAuthService` | `AuthService` | Application interface / Infrastructure service |
| `IPostService` | `BlogPostService` | Application interface / Infrastructure service |
| `ICommentService` | `CommentService` | Application interface / Infrastructure service |
| `ICategoryService` | `CategoryService` | Application interface / Infrastructure service |
| `ICurrentUserService` | `CurrentUserService` | Application interface / API service |
| `IFileService` | `FileService` | Application interface / Infrastructure service |

Infrastructure DI kaydı ilk dört interface/implementasyon çiftini, generic repository'yi ve post repository'yi kaydeder; `ICurrentUserService` API `Program.cs` içinde kaydedilir. `ICommentRepository` interface'i var ancak bu akışta kullanılmıyor; `CommentRepository` sınıfı boş ve DI kaydı yok.

### FluentValidation

| Validator | Servis/request sahibi |
|---|---|
| `RegisterRequestValidator` | `AuthService.RegisterAsync` |
| `LoginRequestValidator` | `AuthService.LoginAsync` |
| `GetPostsRequestValidator` | `BlogPostService.GetAllAsync` |
| `CreatePostRequestValidator` | `BlogPostService.CreateAsync` |
| `UpdatePostRequestValidator` | `BlogPostService.UpdateAsync` |
| `CreateCommentRequestValidator` | `CommentService.CreateAsync` |
| `CreateCategoryRequestValidator` | `CategoryService.CreateAsync` |

Toplam **7 validator**. Diğer CRUD/get-by-id servis metotlarında request validator yok (get-by-id yalnız integer route parametresi alıyor).

### Identity ve JWT

- Seed edilmiş Identity rolleri: **Admin**, **User**.
- Identity minimum parola uzunluğu: **6**; digit, uppercase ve non-alphanumeric zorunlu değil.
- JWT config: `Issuer=BlogAppAPI`, `Audience=BlogAppUsers`, `ExpiryMinutes=60`; secret appsettings'te boş ve dış configuration provider'dan sağlanmalı.
- Token claim'leri: user id, email, full name, username, roller; imza algoritması HS256.
- Doğrulanan alanlar: issuer, audience, token lifetime ve signing key.

### Soft-delete

Global query filter uygulanmış entity sayısı **3**: `Post`, `Comment`, `Category`. Kanıt: [BlogDbContext.cs](./BlogApp.Infrastructure/Persistence/BlogDbContext.cs) satır 41–43.

### Endpoint sayısı ve authorization

Toplam **9 action endpoint**: Auth 2, Posts 5, Comments 1, Categories 1. **6 endpoint authorization altında**: Posts'ta liste harici 4, Comments'ta 1, Admin Categories'te 1. GET posts `[AllowAnonymous]`; auth endpoint'leri anonymous.

### HTTP sonuçları

| Durum | Koddan doğrulanan koşul |
|---|---|
| 400 | FluentValidation hataları middleware ile 400; `[ApiController]` model validation da otomatik 400 sağlayabilir. |
| 401 | Hatalı kullanıcı adı/parola `InvalidCredentialsException` ile 401. `[Authorize]` endpoint'e kimliksiz erişimde JwtBearer challenge 401 beklenir; endpoint'i canlı HTTP olarak bu rapor test etmedi. |
| 403 | Post owner/Admin kuralı ihlalinde middleware 403; authenticated kullanıcı rolü Admin olmayan Categories erişiminde authorization framework 403. |
| 404 | Entity not-found için açık mapping yok; post service generic `Exception` atar ve mevcut middleware bunu **500** yapar. Framework route no-match davranışı ayrı olup burada runtime test edilmedi. |
| 201 | User registration, post create (`CreatedAtAction`), comment create ve category create success. |
| 204 | Post soft-delete success. |

### Veritabanında sayfalama kanıtı

[PostRepository.cs](./BlogApp.Infrastructure/Repositories/PostRepository.cs): `CountAsync` satır 30; page query'de `OrderByDescending`/`ThenByDescending`, `Skip` ve `Take` satır 32–36; `ToListAsync` satır 37. `Skip/Take`, belleğe `ToListAsync` alınmadan önce `IQueryable` zincirindedir.

### Test projesi

- Proje: [BlogApp.Tests.csproj](./BlogApp.Tests/BlogApp.Tests.csproj), xUnit, `net8.0`.
- Test runner son raporunda: **18 geçti, 0 başarısız, 0 atlandı**.
- Test sınıfları: `BlogPostServiceTests` (9 vaka/teori açılımları dahil), `RequestValidatorTests` (4), `AuthServiceTests` (2), `CommentServiceTests` (1), `ExceptionMiddlewareTests` (2).
- Kapsam: non-owner update forbidden, Admin başka postu günceller, create post validation, pagination alt/üst sınırları, register/login/category/comment validator hata durumları, yanlış username/parola exception'ı, 401/403 middleware mapping, comment author current-user context'ten alınır.
- Testler fake repository/identity kullanır; gerçek SQL Server veya uçtan uca JWT authentication testi değildir.

### CI

[ci.yml](./.github/workflows/ci.yml) `BlogApp.sln` yolunu kullanır; `actions/setup-dotnet` ile `8.0.x` kurar; restore, Release build (`--no-restore`) ve Release test (`--no-build`) adımlarını sıralar. Çözüm yolu ve build-test adımları tutarlı görünmektedir. Dosyanın yapılandırması incelendi; GitHub Actions'ta çalıştığına dair run/check kanıtı yoktur.

### Açık kalanlar

- Refresh token, token revoke/rotation akışı yok.
- Upload dosya boyutu/içerik/uzantı doğrulaması ve DB hatasında dosya temizliği görünmüyor.
- Entity not-found 404'e map edilmiyor; mevcut middleware 500 üretiyor.
- Beklenmeyen exception mesajı istemciye çıkabiliyor; middleware'de loglama görünmüyor.
- Comment servisi post existence kontrolü yapmıyor; author claim yoksa `"Unknown"` fallback'i var.
- Kategori uniqueness kontrolü görünmüyor.
- Repository metotları cancellation token almıyor; request cancellation DB çağrılarına taşınmıyor.
- Tarihçede JWT secret alanı boş olmayan değerle bulunmuş; gizli değer açığa çıkarılmadan kaydedilen commit bilgisi yukarıda verilmiştir. Secret rotate edilmesi uygun olur.
- Otomatik migration/DB integration test veya full HTTP integration test görünmüyor.

## İstenen ek repository kontrolleri

- **AutoMapper uyarısı:** `dotnet list BlogApp.sln package --vulnerable --include-transitive` ilk kontrolde Application'da doğrudan, API/Infrastructure/Tests'te geçişli AutoMapper 13.0.1 için High advisory gösterdi. NuGet planı 15.1.3 önerdi; testte API uyumsuzluğu oluşunca mapping açıkça yazılarak paket kaldırıldı. Son tarama bu paket dahil hiçbir projede bilinen açık bulmadı.
- **İç hazırlık dosyası:** `JUNIOR_BACKEND_INTERVIEW_PREP.md` ve benzeri dosyalar Git tarafından takip edilmiyor; dosya çalışma ağacında untracked görünüyordu. İstenildiği gibi değiştirilmedi ve silinmedi. Bu rapor yeni `MULAKAT_HAZIRLIK_RAPORU.md` dosyasıdır.
- **Branch ve commitler:** Çalışma `refactor/no-cqrs` üzerinde tutuldu; `main` değiştirilmedi.
