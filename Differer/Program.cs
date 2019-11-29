using DiffMatchPatch;
using System;
using System.IO;
using System.Collections.Generic;

namespace Differer
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputs = new string[2];
            if(args.Length == 0)
            {
                Console.Write("Target files: ");
                inputs[0] = Console.ReadLine();
                Console.Write("Output: ");
                inputs[1] = Console.ReadLine();
            }
            else
            {
                inputs[0] = args[0];
                inputs[1] = args[1];
            }

            var service = new Service(DiffMatchPatchModule.Default);

            IList<string> files = null;
            using (var reader = new StreamReader(inputs[0]))
            {
                files = reader.ReadToEnd().Replace("\r", "\n").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (var file in files)
            {
                if (File.Exists(file) == false)
                    throw new FileNotFoundException(String.Format("'{0}' is not found.", file), file);
            }
            service.DiffFilesToPrettyHtml(files, inputs.Length == 2 ? inputs[1] : "./");
        }
    }

    class Service
    {
        private DiffMatchPatch.DiffMatchPatch dmp;

        public Service(DiffMatchPatch.DiffMatchPatch dmp)
        {
            this.dmp = dmp;
        }

        public string DiffFilesToPrettyHtml(IList<string> files, string output = "./result.html", bool should_print = true)
        {
            var result = new List<string>();
            var links = new List<string>();
            for (var i = 0; i < files.Count; i++)
            {
                for (var j = i + 1; j < files.Count; j++)
                {
                    var res = DiffFileToPrettyHtml(files[i], files[j], "./", false);
                    var section = Path.GetFileNameWithoutExtension(files[i]) + "___" + Path.GetFileNameWithoutExtension(files[j]);
                    var id = "sec-" + links.Count;
                    res = "<h1 id=\"" + id + "\">" + section + "</h1>" + res;
                    links.Add(String.Format("<li><a href=\"#{0}\">{1}</a></li>", id, section));
                    result.Add(res);
                }
            }

            var html = "<body style='background: #dcd6d9'><h1>Results</h1><ul>" + string.Join("", links) + "</ul>";
            html += string.Join("<br>", result) + "</body>";

            if (!should_print) return html;

            using (var sw = new StreamWriter(output, false))
            {
                sw.Write(html);
            }

            return html;
        }

        public string DiffFileToPrettyHtml(string path1, string path2, string output_dir = "./", bool should_print = true)
        {
            var diffs = dmp.DiffMain(ReadToEnd(path1), ReadToEnd(path2));
            string table_html = MakeHtmlTable(path1, path2, diffs);

            var html = dmp.DiffPrettyHtml(diffs);

            if (Directory.Exists(output_dir) == false)
                throw new DirectoryNotFoundException("Output directory is not found. : " + output_dir);

            var result = table_html + "<h2>Source</h2>" + "<pre>" + html + "</pre>";
            if (!should_print) return result;

            using (var sw = new StreamWriter(Path.Combine(output_dir, MakeFileName(path1, path2)), false))
            {
                sw.Write(result);
            }

            return result;
        }

        private string MakeHtmlTable(string path1, string path2, List<Diff> diffs)
        {
            var table_html = "<h2>Diffs</h2>";
            table_html += String.Format("<table border=\"1\"><thead><tr><th>{0}</th><th>{1}</th></tr></thead><tbody>",
                            Path.GetFileNameWithoutExtension(path1), Path.GetFileNameWithoutExtension(path2));
            Diff pre_diff = new Diff("", Operation.Equal);
            var raw_base = "<tr><td><pre>{0}</pre></td><td><pre>{1}</pre></td></tr>";
            var total = 0;
            foreach (var diff in diffs)
            {
                if (diff.Operation == Operation.Equal) continue;
                if (diff.Operation == Operation.Insert && pre_diff.Operation == Operation.Delete)
                {
                    table_html += String.Format(raw_base, pre_diff.Text, diff.Text);
                    total++;
                }
                else if (diff.Operation == Operation.Insert)
                {
                    table_html += String.Format(raw_base, "", diff.Text);
                    total++;
                }

                if (diff.Operation == Operation.Delete && pre_diff.Operation == Operation.Delete)
                {
                    table_html += String.Format(raw_base, pre_diff.Text, "");
                    total++;
                }
                pre_diff = diff;
            }
            if (pre_diff.Operation == Operation.Delete)
            {
                table_html += String.Format(raw_base, pre_diff.Text, "");
                total++;
            }
            table_html += String.Format("<tr><th>TOTAL</th><th>{0}</th></tr>", total);
            table_html += "</tbody></table>";
            return table_html;
        }

        private string MakeFileName(string path1, string path2)
        {
            const string extension = ".html";
            var prefix = String.Format("{0}____{1}",
                Path.GetFileNameWithoutExtension(path1),
                Path.GetFileNameWithoutExtension(path2));
            var sufix = "";

            while (File.Exists(prefix + sufix + extension))
            {
                if (sufix.Length == 0)
                {
                    sufix = "_1";
                    continue;
                }
                sufix = "_" + (int.Parse(sufix.Substring(1)) + 1);
            }

            return prefix + sufix + extension;
        }

        private string ReadToEnd(string path)
        {
            if (File.Exists(path) == false)
                throw new FileNotFoundException(String.Format("'{0}' is not found.", path), path);
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
