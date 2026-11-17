using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Data;

// ============================================================================
//  KAYNAKLAR: Her konu için okunacak dış ders sayfaları.
//  Anahtar: (Alan adı, Konu sırası) → (Başlık, URL) listesi
// ============================================================================
public static partial class BaslangicVerisi
{
    private const string W3 = "https://www.w3schools.com";
    private const string MsLearn = "https://learn.microsoft.com/tr-tr";

    private static readonly Dictionary<(string Alan, int Sira), (string Baslik, string Url)[]> KaynakListesi = new()
    {
        // ---------------- C# TEMELLERİ ----------------
        [("C# Temelleri", 1)] = [
            ("C# Tutorial (Giriş)", $"{W3}/cs/index.php"),
            ("C# Intro", $"{W3}/cs/cs_intro.php"),
            ("C# Get Started", $"{W3}/cs/cs_getstarted.php"),
            ("C# Syntax", $"{W3}/cs/cs_syntax.php"),
            ("C# Output", $"{W3}/cs/cs_output.php"),
            ("C# Comments", $"{W3}/cs/cs_comments.php"),
        ],
        [("C# Temelleri", 2)] = [
            ("C# Variables", $"{W3}/cs/cs_variables.php"),
            ("C# Data Types", $"{W3}/cs/cs_data_types.php"),
            ("C# Type Casting", $"{W3}/cs/cs_type_casting.php"),
            ("C# User Input", $"{W3}/cs/cs_user_input.php"),
        ],
        [("C# Temelleri", 3)] = [
            ("C# Operators", $"{W3}/cs/cs_operators.php"),
            ("C# Math", $"{W3}/cs/cs_math.php"),
            ("C# Strings", $"{W3}/cs/cs_strings.php"),
            ("C# Booleans", $"{W3}/cs/cs_booleans.php"),
        ],
        [("C# Temelleri", 4)] = [
            ("C# If...Else", $"{W3}/cs/cs_conditions.php"),
            ("C# Switch", $"{W3}/cs/cs_switch.php"),
            ("C# While Loop", $"{W3}/cs/cs_while_loop.php"),
            ("C# For Loop", $"{W3}/cs/cs_for_loop.php"),
            ("C# Break/Continue", $"{W3}/cs/cs_break.php"),
        ],
        [("C# Temelleri", 5)] = [
            ("C# Arrays", $"{W3}/cs/cs_arrays.php"),
            ("List<T> sınıfı (Microsoft Learn)", $"{MsLearn}/dotnet/api/system.collections.generic.list-1"),
            ("Dictionary<TKey,TValue> sınıfı (Microsoft Learn)", $"{MsLearn}/dotnet/api/system.collections.generic.dictionary-2"),
        ],
        [("C# Temelleri", 6)] = [
            ("C# Methods", $"{W3}/cs/cs_methods.php"),
            ("C# Method Parameters", $"{W3}/cs/cs_method_parameters.php"),
            ("C# Method Overloading", $"{W3}/cs/cs_method_overloading.php"),
        ],
        [("C# Temelleri", 7)] = [
            ("C# OOP", $"{W3}/cs/cs_oop.php"),
            ("C# Classes/Objects", $"{W3}/cs/cs_classes.php"),
            ("C# Class Members", $"{W3}/cs/cs_class_members.php"),
            ("C# Constructors", $"{W3}/cs/cs_constructors.php"),
            ("C# Access Modifiers", $"{W3}/cs/cs_access_modifiers.php"),
            ("C# Properties", $"{W3}/cs/cs_properties.php"),
        ],
        [("C# Temelleri", 8)] = [
            ("C# Inheritance", $"{W3}/cs/cs_inheritance.php"),
            ("C# Polymorphism", $"{W3}/cs/cs_polymorphism.php"),
            ("C# Abstraction", $"{W3}/cs/cs_abstract.php"),
            ("C# Interface", $"{W3}/cs/cs_interface.php"),
            ("C# Enums", $"{W3}/cs/cs_enums.php"),
            ("Record türleri (Microsoft Learn)", $"{MsLearn}/dotnet/csharp/fundamentals/types/records"),
        ],
        [("C# Temelleri", 9)] = [
            ("C# Exceptions", $"{W3}/cs/cs_exceptions.php"),
            ("C# Files", $"{W3}/cs/cs_files.php"),
        ],
        [("C# Temelleri", 10)] = [
            ("LINQ'e genel bakış (Microsoft Learn)", $"{MsLearn}/dotnet/csharp/linq/"),
            ("async ve await ile asenkron programlama (Microsoft Learn)", $"{MsLearn}/dotnet/csharp/asynchronous-programming/"),
            ("Lambda ifadeleri (Microsoft Learn)", $"{MsLearn}/dotnet/csharp/language-reference/operators/lambda-expressions"),
        ],

        // ---------------- BACKEND ----------------
        [("Backend", 1)] = [
            ("HTTP Request Methods", $"{W3}/tags/ref_httpmethods.asp"),
            ("HTTP Status Messages", $"{W3}/tags/ref_httpmessages.asp"),
            ("HTTP'ye genel bakış (MDN)", "https://developer.mozilla.org/tr/docs/Web/HTTP/Overview"),
        ],
        [("Backend", 2)] = [
            ("JSON Introduction", $"{W3}/js/js_json_intro.asp"),
            ("JavaScript Fetch API", $"{W3}/js/js_api_fetch.asp"),
            ("ASP.NET Core ile Web API oluşturma (Microsoft Learn)", $"{MsLearn}/aspnet/core/web-api/"),
        ],
        [("Backend", 3)] = [
            ("C# Interface", $"{W3}/cs/cs_interface.php"),
            ("ASP.NET Core'da Dependency Injection (Microsoft Learn)", $"{MsLearn}/aspnet/core/fundamentals/dependency-injection"),
        ],
        [("Backend", 4)] = [
            ("Ortak web uygulaması mimarileri (Microsoft Learn)", $"{MsLearn}/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures"),
        ],
        [("Backend", 5)] = [
            ("ASP.NET Core kimlik doğrulamaya genel bakış (Microsoft Learn)", $"{MsLearn}/aspnet/core/security/authentication/"),
            ("JWT'ye giriş (jwt.io)", "https://jwt.io/introduction"),
        ],
        [("Backend", 6)] = [
            ("ASP.NET Core Middleware (Microsoft Learn)", $"{MsLearn}/aspnet/core/fundamentals/middleware/"),
            ("ASP.NET Core'da loglama (Microsoft Learn)", $"{MsLearn}/aspnet/core/fundamentals/logging/"),
            ("C# Exceptions", $"{W3}/cs/cs_exceptions.php"),
        ],

        // ---------------- SQL ----------------
        [("SQL ve Veritabanı", 1)] = [
            ("SQL Intro", $"{W3}/sql/sql_intro.asp"),
            ("SQL Syntax", $"{W3}/sql/sql_syntax.asp"),
            ("SQL Create Table", $"{W3}/sql/sql_create_table.asp"),
            ("SQL Constraints", $"{W3}/sql/sql_constraints.asp"),
            ("SQL Primary Key", $"{W3}/sql/sql_primarykey.asp"),
            ("SQL Foreign Key", $"{W3}/sql/sql_foreignkey.asp"),
            ("SQL NULL Values", $"{W3}/sql/sql_null_values.asp"),
            ("SQL Data Types", $"{W3}/sql/sql_datatypes.asp"),
        ],
        [("SQL ve Veritabanı", 2)] = [
            ("SQL SELECT", $"{W3}/sql/sql_select.asp"),
            ("SQL WHERE", $"{W3}/sql/sql_where.asp"),
            ("SQL ORDER BY", $"{W3}/sql/sql_orderby.asp"),
            ("SQL AND / OR / NOT", $"{W3}/sql/sql_and.asp"),
            ("SQL LIKE", $"{W3}/sql/sql_like.asp"),
            ("SQL TOP", $"{W3}/sql/sql_top.asp"),
            ("SQL INSERT INTO", $"{W3}/sql/sql_insert.asp"),
            ("SQL UPDATE", $"{W3}/sql/sql_update.asp"),
            ("SQL DELETE", $"{W3}/sql/sql_delete.asp"),
        ],
        [("SQL ve Veritabanı", 3)] = [
            ("SQL Joins", $"{W3}/sql/sql_join.asp"),
            ("SQL INNER JOIN", $"{W3}/sql/sql_join_inner.asp"),
            ("SQL LEFT JOIN", $"{W3}/sql/sql_join_left.asp"),
            ("SQL GROUP BY", $"{W3}/sql/sql_groupby.asp"),
            ("SQL HAVING", $"{W3}/sql/sql_having.asp"),
            ("SQL COUNT", $"{W3}/sql/sql_count.asp"),
            ("SQL Aliases", $"{W3}/sql/sql_alias.asp"),
        ],
        [("SQL ve Veritabanı", 4)] = [
            ("EF Core'a genel bakış (Microsoft Learn)", $"{MsLearn}/ef/core/"),
            ("EF Core migration'lara genel bakış (Microsoft Learn)", $"{MsLearn}/ef/core/managing-schemas/migrations/"),
        ],
        [("SQL ve Veritabanı", 5)] = [
            ("SQL Create Index", $"{W3}/sql/sql_create_index.asp"),
            ("SQL Views", $"{W3}/sql/sql_view.asp"),
            ("SQL Stored Procedures", $"{W3}/sql/sql_stored_procedures.asp"),
        ],
        [("SQL ve Veritabanı", 6)] = [
            ("SQL Injection", $"{W3}/sql/sql_injection.asp"),
            ("EF Core'da transaction'lar (Microsoft Learn)", $"{MsLearn}/ef/core/saving/transactions"),
        ],

        // ---------------- SUNUCU ----------------
        [("Sunucu ve Windows", 1)] = [
            ("ASP.NET Core'u IIS ile barındırma (Microsoft Learn)", $"{MsLearn}/aspnet/core/host-and-deploy/iis/"),
        ],
        [("Sunucu ve Windows", 3)] = [
            ("What is HTTPS?", $"{W3}/whatis/whatis_https.asp"),
        ],
        [("Sunucu ve Windows", 4)] = [
            ("Windows hizmeti olarak .NET uygulaması (Microsoft Learn)", $"{MsLearn}/dotnet/core/extensions/windows-service"),
        ],
        [("Sunucu ve Windows", 6)] = [
            ("PowerShell'e genel bakış (Microsoft Learn)", $"{MsLearn}/powershell/scripting/overview"),
        ],

        // ---------------- YAPAY ZEKA ----------------
        [("Yapay Zeka", 1)] = [
            ("Artificial Intelligence Tutorial", $"{W3}/ai/default.asp"),
            ("Generative AI Tutorial", $"{W3}/gen_ai/index.php"),
        ],
        [("Yapay Zeka", 2)] = [
            ("Microsoft.Extensions.AI kütüphaneleri (Microsoft Learn)", $"{MsLearn}/dotnet/ai/microsoft-extensions-ai"),
        ],
    };

    /// <summary>Anahtara göre Kaynak nesneleri üretir. Kaynak yoksa boş liste.</summary>
    private static List<Kaynak> KaynaklariOlustur(string alan, int sira) =>
        KaynakListesi.TryGetValue((alan, sira), out var liste)
            ? liste.Select((k, i) => new Kaynak
              {
                  Baslik = k.Baslik,
                  Url = k.Url,
                  Site = SiteAdi(k.Url),
                  Sira = i + 1,
              }).ToList()
            : [];

    private static string SiteAdi(string url) =>
        url.Contains("w3schools") ? "W3Schools"
        : url.Contains("learn.microsoft.com") ? "Microsoft Learn"
        : url.Contains("mozilla.org") ? "MDN"
        : new Uri(url).Host;
}
