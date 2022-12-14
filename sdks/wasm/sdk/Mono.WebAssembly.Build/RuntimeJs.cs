//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mono.WebAssembly.Build {
    using System.Collections.Generic;
    using System;
    
    
    public partial class RuntimeJs : TemplateBase {
        
        
        #line 21 ""

public string VfsPrefix { get; set; }
public string DeployPrefix { get; set; }
public bool EnableDebugging { get; set; }
public IEnumerable<string> FileList { get; set; }

        #line default
        #line hidden
        
        
        public override string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 3 ""
            this.Write("var Module = { \n\tonRuntimeInitialized: function () {\n\t\tMONO.mono_load_runtime_and" +
                    "_bcl (\n\t\t\t\"");
            
            #line default
            #line hidden
            
            #line 6 ""
            this.Write(this.ToStringHelper.ToStringWithCulture(VfsPrefix??"managed"));
            
            #line default
            #line hidden
            
            #line 6 ""
            this.Write("\",\n\t\t\t\"");
            
            #line default
            #line hidden
            
            #line 7 ""
            this.Write(this.ToStringHelper.ToStringWithCulture(DeployPrefix??"managed"));
            
            #line default
            #line hidden
            
            #line 7 ""
            this.Write("\",\n\t\t\t");
            
            #line default
            #line hidden
            
            #line 8 ""
            this.Write(this.ToStringHelper.ToStringWithCulture(EnableDebugging?1:0));
            
            #line default
            #line hidden
            
            #line 8 ""
            this.Write(",\n\t\t\t[ \"");
            
            #line default
            #line hidden
            
            #line 9 ""
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Join("\", \"", FileList)));
            
            #line default
            #line hidden
            
            #line 9 ""
            this.Write("\" ],\n\t\t\tfunction () {\n");
            
            #line default
            #line hidden
            
            #line 11 ""
            
            #line default
            #line hidden
            
            #line 14 ""
            this.Write("\t\t\t\tApp.init ();\n\t\t\t}\n\t\t);\n\t},\n};\n\n\n");
            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        public override void Initialize() {
            base.Initialize();
        }
    }
}
