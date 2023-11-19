using System.Text.RegularExpressions;
using CodeGen.Mappings;

namespace CodeGen.Services {
    public class CsToTsService {
        private readonly string _in_path;
        private List<string> _lines = new List<string>();

        public CsToTsService(string in_path) {
            this._in_path = in_path;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ConvertToTypeScriptAsync() {
            
            try {
                if (File.Exists(this._in_path)) {
                    var lines = await File.ReadAllLinesAsync(this._in_path);

                    if (lines is not null) {
                        this._lines = lines.ToList();

                        // Get classname
                        var class_name = this.GetClassName();

                        if (!string.IsNullOrEmpty(class_name)) {
                            // Mapp props
                            var prop_mappings = this.MapProps(class_name);
                            var export_data = this.CreateContentsFromMappings(prop_mappings, class_name);

                            var out_path = this._in_path.Replace(".cs", ".ts");
                            await File.WriteAllLinesAsync(out_path, export_data);
                            Console.WriteLine($"Exported to TypeScript {out_path}");
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Returns typescript type for given CS type
        /// </summary>
        /// <param name="cs_type">CS type</param>
        /// <returns>TypeScript type</returns>
        private string GetTSType(string cs_type) {

            var is_nullable = cs_type.Contains("?");

            switch (cs_type.Replace("?", "")) {
                case "int":
                    return is_nullable ? "number | null" : "number";

                case "string":
                    return is_nullable ? "string | null" : "string";

                case "DateTime":
                    return is_nullable ? "string | null" : "string";

                default:
                    var def = cs_type.Replace("?", "");
                    return is_nullable ? $"{def} | null" : def;
            }
        }

        /// <summary>
        /// Returns typescript default assignment
        /// </summary>
        /// <param name="cs_type">cs type</param>
        /// <returns>string</returns>
        private string GetTSDefault(string cs_type) {

            var is_nullable = cs_type.Contains("?");

            switch (cs_type.Replace("?", "")) {
                case "int":
                    return is_nullable ? ": number | null = null;" : " = 0;";

                case "string":
                    return is_nullable ? ": string | null = null;" : " = \"\";";

                case "DateTime":
                    return is_nullable ? ": string | null = null;" : " = new Date().toISOString();";

                default:
                    var def = cs_type.Replace("?", "");
                    return is_nullable ? $": {def} | null = null;" : $" = new {def}();";
            }
        }

        /// <summary>
        /// Returns classname
        /// </summary>
        /// <returns>string | null</returns>
        private string? GetClassName() {
            
            string? class_name = null;

            try {
                var name_line = this._lines.FirstOrDefault(e => e.Contains("class"));

                if (name_line is not null) {
                    var regex = new Regex("(?<=class\\s+)\\w+");
                    var match = regex.Match(name_line);

                    if (match is not null) {
                        class_name = match.Value;
                        class_name = class_name.Replace("{", "");
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while parsing class name: {ex.Message}");
            }

            return class_name;
        }


        /// <summary>
        /// Mapps CS class props
        /// </summary>
        /// <param name="class_name">CS Classname</param>
        /// <returns>PropMapping[]</returns>
        private List<PropMapping> MapProps(string class_name) {

            var mappings = new List<PropMapping>();

            try {
                var map_lines = this._lines
                    .Where(e => !e.Contains(class_name))
                    .Where(e => e.Contains("public"));
                
                foreach (var line in map_lines) {
                    var line_split = line.Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToList();

                    if (line_split.Count >= 3) {
                        mappings.Add(new PropMapping() {
                            prop_type = this.GetTSType(line_split[1]),
                            prop_name = line_split[2],
                            prop_def = this.GetTSDefault(line_split[1])
                        });
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while mapping props {ex.Message}");
            }

            return mappings;
        }

        /// <summary>
        /// Creates lines for export file from mappings
        /// </summary>
        /// <param name="mappings">PropMapping[]</param>
        /// <param name="class_name">class name</param>
        /// <returns>string[]</returns>
        private List<string> CreateContentsFromMappings(List<PropMapping> mappings, string class_name) {

            var file_lines = new List<string>();

            try {
                // Begin with interface
                file_lines.Add("interface I" + class_name + " {");

                foreach (var mapping in mappings) {
                    file_lines.Add($"\t{mapping.prop_name}: {mapping.prop_type};");
                }

                file_lines.Add("}");
                file_lines.Add("");

                // Create class template
                file_lines.Add("export class " + class_name + " implements I" + class_name + " {");
                file_lines.Add("");

                foreach (var mapping in mappings) {
                    file_lines.Add($"\tpublic {mapping.prop_name}{mapping.prop_def}");
                }

                file_lines.Add("");
                file_lines.Add("\tpublic static CreateInstance(source: I" + class_name + "): " + class_name + " {");
                file_lines.Add($"\t\tconst instance = new {class_name}();");
                file_lines.Add("\t\tObject.assign(instance, source);");
                file_lines.Add("\t\treturn instance;");
                file_lines.Add("\t}");
                file_lines.Add("}");
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while creating new file contents from mappings: {ex.Message}");
            }

            return file_lines;
        }
    }
}