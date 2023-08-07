using Bi.Core.Models;
using Bi.Entities.Input;
using Bi.Entities.Response;
using Bi.Services.IService;
using FluentFTP;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Bi.Services.Service;

public class DynamicCodeService : IDynamicCodeService
{
    private readonly ILogger<DynamicCodeService> logger;

    public DynamicCodeService(ILogger<DynamicCodeService> logger){
        this.logger = logger;
    }

    public async Task<(string,bool)> syntaxRules(DynamicCodeInput input)
    {
        var res =await Task.Run(()=>executeCode(input));
        return res;
    }

    private  (string,bool) executeCode(DynamicCodeInput input)
    {
        string codeToCompile = input.DynamicCode;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

        string assemblyName = Path.GetRandomFileName();

        string basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location) ?? "";
        var refPaths = new[] {
            typeof(System.Object).GetTypeInfo().Assembly.Location,
            typeof(JToken).GetTypeInfo().Assembly.Location,
            typeof(CellItem).GetTypeInfo().Assembly.Location,
            typeof(IFtpClient).GetTypeInfo().Assembly.Location,
            typeof(PageEntity<>).GetTypeInfo().Assembly.Location,
            typeof(StringBuilder).GetTypeInfo().Assembly.Location,
            Path.Combine(basePath, "System.dll"),
            Path.Combine(basePath, "System.Linq.dll"),
            Path.Combine(basePath, "System.Data.dll"),
            Path.Combine(basePath, "System.Runtime.dll"),
            Path.Combine(basePath, "System.Reflection.dll"),
            Path.Combine(basePath, "System.Collections.dll"),
            Path.Combine(basePath, "System.Data.Common.dll")
        };
        MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);
            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                StringBuilder sb = new StringBuilder(); 
                foreach (Diagnostic diagnostic in failures)
                {
                    sb.Append($"\t\n{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                logger.LogInformation(sb.ToString());
                return (sb.ToString(),false);
            }
            else
            {
                if(input.CheckFlag)
                {
                    return ("语法检查无误！",true);
                }
                ms.Seek(0, SeekOrigin.Begin);
                try
                {
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("RoslynCompileSample.Writer");
                    var instance = assembly.CreateInstance("RoslynCompileSample.Writer");
                    var meth = type.GetMember("Write").First() as MethodInfo;
                    if (meth != null && instance != null)
                    {
                        object obj = meth.Invoke(instance, new[] { input.List });
                        if(obj != null && typeof(String) == obj.GetType() && obj.ToString() != "OK")
                        {
                            return (obj.ToString(), false);
                        }
                        else
                        {
                            return ("OK", true);
                        }
                    }
                    return ("执行报错", false);
                }
                catch(Exception ex)
                {
                    return ("执行报错:"+ ex.ToString().Substring(0,100), false);
                }
                    
            }
                
        }
    }
}
