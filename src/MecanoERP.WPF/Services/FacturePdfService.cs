using MecanoERP.Core.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace MecanoERP.Infrastructure.Services;

/// <summary>Génère les PDFs de factures via QuestPDF.</summary>
public static class FacturePdfService
{
    static FacturePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GenererFacturePdf(Facture facture)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, facture));
                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(9);
                        x.Span(" / ").FontSize(9).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(9);
                    });
            });
        });

        return doc.GeneratePdf();

        void ComposeHeader(IContainer c)
        {
            c.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("MécanoERP").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Garage Automobile").FontSize(10).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(150).Column(col =>
                {
                    col.Item().AlignRight().Text("FACTURE").FontSize(18).Bold();
                    col.Item().AlignRight().Text($"N° {facture.Numero}").FontSize(12);
                    col.Item().AlignRight().Text($"Date : {facture.DateFacture:dd/MM/yyyy}").FontSize(10);
                });
            });
        }

        void ComposeContent(IContainer c, Facture f)
        {
            c.PaddingTop(20).Column(col =>
            {
                // Client
                col.Item().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(clientCol =>
                    {
                        clientCol.Item().Text("FACTURER À :").FontSize(9).FontColor(Colors.Grey.Medium);
                        clientCol.Item().Text(f.Client?.Nom ?? "").Bold();
                        clientCol.Item().Text(f.Client?.Email ?? "");
                        clientCol.Item().Text(f.Client?.Telephone ?? "");
                    });
                    row.RelativeItem().Column(otCol =>
                    {
                        otCol.Item().Text("ORDRE DE TRAVAIL :").FontSize(9).FontColor(Colors.Grey.Medium);
                        otCol.Item().Text(f.OrdreTravail?.Numero ?? "").Bold();
                        otCol.Item().Text($"Véhicule : {f.OrdreTravail?.Vehicule?.Marque} {f.OrdreTravail?.Vehicule?.Modele}");
                        otCol.Item().Text($"Immat. : {f.OrdreTravail?.Vehicule?.Immatriculation}");
                    });
                });

                col.Item().PaddingTop(20);

                // Tableau
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(4);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    // En-tête
                    static IContainer HeaderCell(IContainer c) =>
                        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                         .Background(Colors.Blue.Darken2).Padding(6);
                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Description");
                        h.Cell().Element(HeaderCell).AlignCenter().Text("Qté");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Prix unit.");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                    });

                    // Lignes
                    bool alt = false;
                    foreach (var ligne in f.OrdreTravail?.Lignes ?? [])
                    {
                        var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        static IContainer DataCell(IContainer c, string bg) =>
                            c.Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        table.Cell().Element(c => DataCell(c, bg)).Text(ligne.Description);
                        table.Cell().Element(c => DataCell(c, bg)).AlignCenter().Text($"{ligne.Quantite}");
                        table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"{ligne.PrixUnitaire:C}");
                        table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"{ligne.Total:C}");
                        alt = !alt;
                    }
                });

                col.Item().PaddingTop(20).AlignRight().Column(totaux =>
                {
                    totaux.Item().Row(r =>
                    {
                        r.ConstantItem(150).AlignLeft().Text("Sous-total :").Bold();
                        r.ConstantItem(100).AlignRight().Text($"{f.MontantHT:C}");
                    });
                    totaux.Item().Row(r =>
                    {
                        r.ConstantItem(150).AlignLeft().Text("TPS (5%) :");
                        r.ConstantItem(100).AlignRight().Text($"{f.MontantTPS:C}");
                    });
                    totaux.Item().Row(r =>
                    {
                        r.ConstantItem(150).AlignLeft().Text("TVQ (9.975%) :");
                        r.ConstantItem(100).AlignRight().Text($"{f.MontantTVQ:C}");
                    });
                    totaux.Item().Background(Colors.Blue.Lighten4).Padding(6).Row(r =>
                    {
                        r.ConstantItem(150).AlignLeft().Text("TOTAL TTC :").Bold().FontSize(12);
                        r.ConstantItem(100).AlignRight().Text($"{f.MontantTTC:C}").Bold().FontSize(12);
                    });
                    if (f.MontantPaye > 0)
                    {
                        totaux.Item().Row(r =>
                        {
                            r.ConstantItem(150).AlignLeft().Text("Montant payé :").FontColor(Colors.Green.Darken1);
                            r.ConstantItem(100).AlignRight().Text($"{f.MontantPaye:C}").FontColor(Colors.Green.Darken1);
                        });
                        totaux.Item().Background(Colors.Orange.Lighten3).Padding(6).Row(r =>
                        {
                            r.ConstantItem(150).AlignLeft().Text("SOLDE DÛ :").Bold();
                            r.ConstantItem(100).AlignRight().Text($"{f.SoldeRestant:C}").Bold();
                        });
                    }
                });

                col.Item().PaddingTop(30).Text("Merci de votre confiance !").Italic().FontColor(Colors.Grey.Medium);
            });
        }
    }

    public static void SauvegarderPdf(Facture facture, string cheminFichier)
    {
        var bytes = GenererFacturePdf(facture);
        File.WriteAllBytes(cheminFichier, bytes);
    }
}
