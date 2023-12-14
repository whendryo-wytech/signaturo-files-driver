using iText.Forms;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Pdfa;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations;

public class DriverSignPDF
{
    public static void Main(string[] args)
    {

        Dictionary<string, string> paramsList = new Dictionary<string, string>();
        foreach (string item in args)
        {
            string[] split = item.Split('=');
            if(split.Count() == 2)
            {
                paramsList.Add(split[0].TrimStart('-'), split[1].Trim('"'));
            }
        }

        //--type=split --file-origin="C:\Users\whend\source\repos\DriverSignPDF\storage\QRCOde.PDF"
        //--type=merge --merge-path="C:\Users\whend\source\repos\DriverSignPDF\storage" --merge-output="C:\Users\whend\source\repos\DriverSignPDF\storage\teste.pdf" --file-origin="\\wsl.localhost\Ubuntu\home\whendryo\acessoportal\app\storage\app\private\sign\tenant\demo\41df4621-f87f-41c9-9f40-38e3ee768c01\origin.pdf" --file-proof="\\wsl.localhost\Ubuntu\home\whendryo\acessoportal\app\storage\app\private\sign\tenant\demo\41df4621-f87f-41c9-9f40-38e3ee768c01\proof.pdf" --file-output="\\wsl.localhost\Ubuntu\home\whendryo\acessoportal\app\storage\app\private\sign\tenant\demo\41df4621-f87f-41c9-9f40-38e3ee768c01\signed.pdf" --file-title="teste" --file-author="whendryo@wytech.com.br" --id=41df4621-f87f-41c9-9f40-38e3ee768c01 --id-sign=41df4621-f87f-41c9-9f40-38e3ee768c01-d8c312e3-70fa-43b6-b30a-c68bb6e8661a-fc910fa0-22d4-48cf-8719-6de06ca939d9 --font-path="\\wsl.localhost\Ubuntu\home\whendryo\acessoportal\app\storage\fonts\roboto\Roboto-Regular.ttf" --keystore-path="\\wsl.localhost\Ubuntu\home\whendryo\acessoportal\app\storage\app\private\sign\certificate\acessoportal\certificado.pfx" --keystore-password="12345"
        //--type=add-keystore --keystore-password="1234" --keystore-path="C:\Users\whend\OneDrive\Área de Trabalho\cert\IFL.pfx" --file-origin="C:\Users\whend\OneDrive\Área de Trabalho\teste\arquivo.pdf" --file-output="C:\Users\whend\OneDrive\Área de Trabalho\teste\arquivo.pdf"
        
        string fileOrigin, fileProof, fileOutput, fileTiltle, fileAuthor, fontPath, keystorePath, keystorePassword, keystoreTenantPath, keystoreTenantPassword, 
        hashSign, uuidDoc, mergePath, mergeOutput;

        string type;
        paramsList.TryGetValue("type", out type);

        if(type == "help")
        {
            Console.WriteLine("--type : Tipo de execuções aceitas: \n\n     \"help\" - Listagem de comandos \n     \"sign-proof\" - Geração de comprovante de assinatura eletrônica \n     \"merge\" - Unificação de PDFs \n");
            Console.WriteLine("--file-origin : Arquivo original PDF");
            Console.WriteLine("--file-proof : Arquivo com o comprovante de assinatura");
            Console.WriteLine("--file-output : Nome do arquivo que será gerado pela rotina de assinatura");
            Console.WriteLine("--file-title : Título do documento que será colocado nas propriedades do PDF");
            Console.WriteLine("--file-author : Autor do arquivo que será colocado nas propriedades do PDF");
            Console.WriteLine("--font-path : Arquivo com a fonte que será utilizada para o rodapé da assinatura");
            Console.WriteLine("--keystore-path : Arquivo do certificado digital");
            Console.WriteLine("--keystore-password : Senha do certificado digital");
            Console.WriteLine("--keystore-tenant-path : Arquivo do certificado digital do cliente (tenant)");
            Console.WriteLine("--keystore-tenant-password : Senha do certificado digital do cliente (tenant)");
            Console.WriteLine("--id-sign : Hash da assinatura");
            Console.WriteLine("--id : Uuid do documento");
            Console.WriteLine("--merge-path : Pasta com os arquivos que serão unificados");
            Console.WriteLine("--merge-output : Nome do arquivo que será gerado apartir da unificação");
            return;
        }

        if(type == "split")
        {
            paramsList.TryGetValue("file-origin", out fileOrigin);
            if (fileOrigin == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-origin\" (arquivo de origem) precisa ser informado");
            }
            new DriverSignPDF().splitFile(fileOrigin);
            return;
        }

        if(type == "merge")
        {
            paramsList.TryGetValue("merge-path", out mergePath);
            if (mergePath == null)
            {
                throw new InvalidOperationException($"O parâmetro \"merge-path\" (pasta de origem) precisa ser informado");
            }
            paramsList.TryGetValue("merge-output", out mergeOutput);
            if (mergeOutput == null)
            {
                throw new InvalidOperationException($"O parâmetro \"merge-output\" (arquivo de saída) precisa ser informado");
            }
            new DriverSignPDF().mergeFiles(mergePath,mergeOutput);
            return; 
        }

        if (type == "add-keystore")
        {
            paramsList.TryGetValue("file-origin", out fileOrigin);
            if (fileOrigin == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-origin\" (arquivo de origem) precisa ser informado");
            }
            paramsList.TryGetValue("file-output", out fileOutput);
            if (fileOutput == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-output\" (arquivo assinado) precisa ser informado");
            }
            paramsList.TryGetValue("keystore-path", out keystorePath);
            if (keystorePath == null)
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-path\" (Certificado digital) precisa ser informado");
            }
            paramsList.TryGetValue("keystore-password", out keystorePassword);
            if (keystorePassword == null)
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-password\" (Senha do certificado digital) precisa ser informado");
            }
            new DriverSignPDF().addCertificatePDF(fileOrigin, fileOutput, keystorePath, keystorePassword);
            return;
        }

        if ((type == null)||(type == "sign-proof"))
        {
            paramsList.TryGetValue("file-origin", out fileOrigin);
            if (fileOrigin == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-origin\" (arquivo de origem) precisa ser informado");
            }

            paramsList.TryGetValue("file-proof", out fileProof);
            if (fileProof == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-proof\" (comprovante de assinatura) precisa ser informado");
            }

            paramsList.TryGetValue("file-output", out fileOutput);
            if (fileOutput == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-output\" (arquivo assinado) precisa ser informado");
            }

            paramsList.TryGetValue("file-title", out fileTiltle);
            if (fileTiltle == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-title\" (Título do arquivo) precisa ser informado");
            }

            paramsList.TryGetValue("file-author", out fileAuthor);
            if (fileAuthor == null)
            {
                throw new InvalidOperationException($"O parâmetro \"file-author\" (Autor do arquivo) precisa ser informado");
            }

            paramsList.TryGetValue("font-path", out fontPath);
            if (fontPath == null)
            {
                throw new InvalidOperationException($"O parâmetro \"font-path\" (Arquivo da fonte) precisa ser informado");
            }

            paramsList.TryGetValue("keystore-path", out keystorePath);
            if (keystorePath == null)
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-path\" (Certificado digital) precisa ser informado");
            }

            paramsList.TryGetValue("keystore-password", out keystorePassword);
            if (keystorePassword == null)
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-password\" (Senha do certificado digital) precisa ser informado");
            }

            paramsList.TryGetValue("keystore-tenant-path", out keystoreTenantPath);
            paramsList.TryGetValue("keystore-tenant-password", out keystoreTenantPassword);

            if ((keystoreTenantPath != null)&&(keystoreTenantPassword == null))
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-tenant-password\" (Senha do certificado digital) precisa ser informado quando fornecido o parâmetro \"keystore-tenant-path\"");
            }
            if ((keystoreTenantPath == null) && (keystoreTenantPassword != null))
            {
                throw new InvalidOperationException($"O parâmetro \"keystore-tenant-path\" (Certificado digital) precisa ser informado quando fornecido o parâmetro \"keystore-tenant-password\"");
            }

            paramsList.TryGetValue("id-sign", out hashSign);
            if (hashSign == null)
            {
                throw new InvalidOperationException($"O parâmetro \"id-sign\" (Hash da assinatura) precisa ser informado");
            }

            paramsList.TryGetValue("id", out uuidDoc);
            if (uuidDoc == null)
            {
                throw new InvalidOperationException($"O parâmetro \"id\" (UUID do documento) precisa ser informado");
            }

            string text = $"Signaturo \n Documento: {uuidDoc} \n Assinatura: {hashSign}";
            int fontSize = 7;

            //Cria o PDF assinado
            new DriverSignPDF().addFooterPDF(fileOrigin, fileOutput, text, fontPath, fontSize);
            new DriverSignPDF().addProofPDF(fileOutput, fileProof, fileOutput, fileTiltle, fileAuthor);
            new DriverSignPDF().addCertificatePDF(fileOutput, fileOutput, keystorePath, keystorePassword);

            if (keystoreTenantPath != null)
            {
                new DriverSignPDF().addCertificatePDF(fileOutput, fileOutput, keystoreTenantPath, keystoreTenantPassword);
            }

            return;
        }

    }

    protected void splitFile(string path)
    {
        validateFile(path);

        PdfDocument pdfDoc = new PdfDocument(new PdfReader(path));

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            string file = path + "-" + i.ToString() + ".pdf";
            PdfDocument filePdf = new PdfDocument(new PdfWriter(file));
            pdfDoc.CopyPagesTo(i, i, filePdf, 1, new PdfPageFormCopier());
            filePdf.Close();
        }

        pdfDoc.Close();

    }
    protected void validateFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"O arquivo {path} não existe!");
        }
    }

    protected void mergeFiles(string path, string dest)
    {

        PdfDocument pdfDoc = new PdfDocument(new PdfWriter(new FileStream(dest, FileMode.Create)));

        foreach (string file in Directory.GetFiles(path, "*.pdf", SearchOption.AllDirectories))
        {
            if(file != dest)
            {
                PdfDocument filePdf = new PdfDocument(new PdfReader(file));
                filePdf.CopyPagesTo(1, filePdf.GetNumberOfPages(), pdfDoc, pdfDoc.GetNumberOfPages() + 1, new PdfPageFormCopier());
            }
        }
        pdfDoc.Close();

    }

    protected void addCertificatePDF(string source, string dest, string keystore, string password)
    {
        validateFile(source);
        validateFile(keystore);

        Pkcs12Store pk12 = new Pkcs12Store(new FileStream(keystore, FileMode.Open, FileAccess.Read), password.ToCharArray());
        string alias = null;
        foreach (object a in pk12.Aliases){
            alias = ((string)a);
            if (pk12.IsKeyEntry(alias)){
                break;
            }
        }

        ICipherParameters pk = pk12.GetKey(alias).Key;
        X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
        X509Certificate[] chain = new X509Certificate[ce.Length];
        for (int k = 0; k < ce.Length; ++k){
            chain[k] = ce[k].Certificate;
        }


        string tmp = source + ".tmp.pdf";
        File.Copy(source, tmp, true);
        PdfReader reader = new PdfReader(source);
        PdfSigner signer = new PdfSigner(reader,new FileStream(tmp, FileMode.Create),new StampingProperties().UseAppendMode());
        IExternalSignature pks = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);
        signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
        reader.Close();

        File.Delete(dest);
        File.Move(tmp, dest, true);

    }

    protected void addFooterPDF(string source, string dest, string text, string fontPath, int fontSize)
    {
        validateFile(source);
        validateFile(fontPath);

        PdfDocument pdfDoc = new PdfDocument(new PdfReader(source), new PdfWriter(dest));
        Document doc = new Document(pdfDoc);
        Paragraph footer = new Paragraph(text).SetFont(PdfFontFactory
                            .CreateFont(fontPath))
                            .SetFontSize(fontSize)
                            .SetFontColor(ColorConstants.GRAY);

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) { 
            Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
            float coordX = ((pageSize.GetLeft() + doc.GetLeftMargin()) + (pageSize.GetRight() - doc.GetRightMargin())) / 2;
            float footerY = doc.GetBottomMargin() - 10;
            doc.ShowTextAligned(footer, coordX, footerY, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
        }

        doc.Close();

    }

    protected void addProofPDF(string source, string proof, string dest, string documentTitle, string documentAuthor)
    {

        validateFile(source);
        validateFile(proof);

        validateFile(source);
        validateFile(proof);

        string tmp = source + ".tmp.pdf";
        PdfDocument pdfDoc = new PdfDocument(new PdfWriter(new FileStream(tmp, FileMode.Create)));
        PdfDocument filePdf = new PdfDocument(new PdfReader(source));
        filePdf.CopyPagesTo(1, filePdf.GetNumberOfPages(), pdfDoc, pdfDoc.GetNumberOfPages() + 1, new PdfPageFormCopier());
        filePdf.Close();
        filePdf = new PdfDocument(new PdfReader(proof));
        filePdf.CopyPagesTo(1, filePdf.GetNumberOfPages(), pdfDoc, pdfDoc.GetNumberOfPages() + 1, new PdfPageFormCopier());
        PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
        info.SetTitle(documentTitle);
        info.SetAuthor(documentAuthor);
        info.SetSubject("Signaturo - Assinatura Eletrônica");
        info.SetCreator("Signaturo");
        pdfDoc.Close();

        File.Delete(dest);
        File.Copy(tmp, dest);

        /*
        string tmp = dest + ".tmp.pdf";
        File.Copy(source, tmp, true);

        PdfDocument pdfDoc = new PdfDocument(new PdfReader(source), new PdfWriter(tmp));
        PdfDocument cover = new PdfDocument(new PdfReader(proof));

        PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
        info.SetTitle(documentTitle);
        info.SetAuthor(documentAuthor);
        info.SetSubject("Signaturo - Assinatura Eletrônica");
        info.SetCreator("Signaturo");
       
        cover.CopyPagesTo(1, cover.GetNumberOfPages(), pdfDoc, pdfDoc.GetNumberOfPages() + 1, new PdfPageFormCopier());

        cover.Close();
        pdfDoc.Close();

        File.Delete(dest);
        File.Move(tmp, dest);
        */

    }



}