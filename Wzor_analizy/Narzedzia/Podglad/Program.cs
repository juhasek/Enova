using DevExpress.XtraRichEdit;
using DevExpress.Pdf;
var src = args[0]; var outDir = args[1]; var pages = args.Length > 2 ? args[2] : "1";
using var srv = new RichEditDocumentServer();
srv.LoadDocument(src);
var pdf = Path.Combine(outDir, Path.GetFileNameWithoutExtension(src) + ".pdf");
using (var fs = File.Create(pdf)) srv.ExportToPdf(fs);
using var proc = new PdfDocumentProcessor();
proc.LoadDocument(pdf);
Console.WriteLine($"pages={proc.Document.Pages.Count}");
foreach (var p in pages.Split(',')) {
  int i = int.Parse(p); if (i > proc.Document.Pages.Count) continue;
  using var bmp = proc.CreateBitmap(i, 1400);
  bmp.Save(Path.Combine(outDir, Path.GetFileNameWithoutExtension(src) + $"_p{i}.png"));
}
