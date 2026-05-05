using System.Reflection;
using System.IO.Compression;
using Expedita.Export.Excel;

static void DumpType(Type type)
{
    Console.WriteLine($"\n=== {type.FullName} ===");

    var nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
    if (nested.Length > 0)
    {
        Console.WriteLine("-- Nested Types --");
        foreach (var nt in nested.OrderBy(t => t.FullName))
        {
            Console.WriteLine(nt.FullName);
            if (nt.IsEnum)
            {
                foreach (var name in Enum.GetNames(nt))
                {
                    Console.WriteLine($"  - {name} = {Convert.ToInt32(Enum.Parse(nt, name))}");
                }
            }
        }
    }

    Console.WriteLine("-- Constructors --");
    foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
    {
        Console.WriteLine(ctor);
    }

    Console.WriteLine("-- Properties --");
    foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).OrderBy(p => p.Name))
    {
        Console.WriteLine($"{prop.PropertyType.FullName} {prop.Name} {{ {(prop.CanRead ? "get;" : string.Empty)} {(prop.CanWrite ? "set;" : string.Empty)} }}");
    }

    Console.WriteLine("-- Fields --");
    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).OrderBy(f => f.Name))
    {
        Console.WriteLine($"{field.FieldType.FullName} {field.Name}");
    }

    Console.WriteLine("-- Methods --");
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                 .Where(m => !m.IsSpecialName)
                 .OrderBy(m => m.Name))
    {
        Console.WriteLine(method);
    }
}

var typesToDump = new[]
{
    "Expedita.Export.Excel.xlsxDocumento",
    "Expedita.Export.Excel.xlsxPagina",
    "Expedita.Export.Excel.xlsxCelula",
};

// Force-load the assemblies so the types are discoverable even if we never reference them directly.
try { _ = Assembly.Load("Expedita.Export.Excel"); } catch { }
try { _ = Assembly.Load("Expedita.Runtime"); } catch { }

foreach (var typeName in typesToDump)
{
    var type = Type.GetType(typeName);
    if (type is null)
    {
        // Scan all loaded assemblies to find it.
        type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(typeName, throwOnError: false))
            .FirstOrDefault(t => t is not null);
    }

    if (type is null)
    {
        Console.WriteLine($"Type not found: {typeName}");
        continue;
    }

    DumpType(type);
}

// Smoke test: generate a tiny XLSX and print a snippet of sheet XML.
try
{
    var outDir = Path.Combine(AppContext.BaseDirectory, "out");
    Directory.CreateDirectory(outDir);

    var tempDir = Path.Combine(outDir, "tmp");
    Directory.CreateDirectory(tempDir);

    var doc = new xlsxDocumento(tempDir);
    var page = doc.AdicionarPagina("Evolucao", 10, 5);

    page.set_Celula(1, 1, new xlsxCelula(1, 1) { valor = "Data", tipoValor = xlsxCelula.tiposValor.TextoHeader });
    page.set_Celula(2, 1, new xlsxCelula(2, 1) { valor = "Hora", tipoValor = xlsxCelula.tiposValor.TextoHeader });
    page.set_Celula(3, 1, new xlsxCelula(3, 1) { valor = "Direcao", tipoValor = xlsxCelula.tiposValor.TextoHeader });
    page.set_Celula(4, 1, new xlsxCelula(4, 1) { valor = "Qtd", tipoValor = xlsxCelula.tiposValor.TextoHeader });

    page.set_Celula(1, 2, new xlsxCelula(1, 2) { valor = "04/05", tipoValor = xlsxCelula.tiposValor.Texto });
    page.set_Celula(2, 2, new xlsxCelula(2, 2) { valor = "10", tipoValor = xlsxCelula.tiposValor.Texto });
    page.set_Celula(3, 2, new xlsxCelula(3, 2) { valor = "0", tipoValor = xlsxCelula.tiposValor.Inteiro });
    page.set_Celula(4, 2, new xlsxCelula(4, 2) { valor = "123", tipoValor = xlsxCelula.tiposValor.Inteiro });

    Stream? stream = null;
    var fileName = "smoke.xlsx";
    doc.Exportar(fileName, xlsxDocumento.tipoExportacao.documentoXls, outDir, ref stream);
    stream?.Flush();
    stream?.Dispose();

    var xlsxPath = Path.Combine(outDir, fileName);
    Console.WriteLine($"\nGenerated: {xlsxPath}");

    using var zip = ZipFile.OpenRead(xlsxPath);
    var entry = zip.GetEntry("xl/worksheets/sheet1.xml") ?? zip.Entries.FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase));
    if (entry != null)
    {
        using var reader = new StreamReader(entry.Open());
        var xml = reader.ReadToEnd();
        Console.WriteLine("\n-- sheet xml snippet --");
        Console.WriteLine(xml.Length > 600 ? xml[..600] : xml);
    }
    else
    {
        Console.WriteLine("No sheet xml found in generated xlsx.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Smoke test failed: {ex}");
}
