using Ponto.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// NOVA CLASSE AUXILIAR PARA O PDF
public class LinhaPdfDia
{
    public DateTime Data { get; set; }
    public string Ent1 { get; set; } = "-";
    public string Sai1 { get; set; } = "-";
    public string Ent2 { get; set; } = "-";
    public string Sai2 { get; set; } = "-";
}

public class RelatorioPdfService
{
    // AGORA RECEBE A LISTA DE LinhaPdfDia
    public byte[] GerarRelatorioMensal(string nomeFuncionario, string mesAno, List<LinhaPdfDia> diasTrabalhados)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Cabeçalho
                page.Header().Column(col =>
                {
                    // CORRIGIDO: Colors.Blue.Darken2 em vez de Colors.BlueDarken2
                    col.Item().Text("Registo de Tempos de Trabalho").SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Colaborador: {nomeFuncionario}");
                    col.Item().Text($"Mês/Ano: {mesAno}");
                    col.Item().PaddingBottom(1, Unit.Centimetre);
                });

                // Tabela de Pontos
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Padding(2).Text("Data").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Ent 1").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Sai 1").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Ent 2").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Sai 2").SemiBold();
                    });

                    // CORRIGIDO: Mapeando os dados agrupados
                    foreach (var dia in diasTrabalhados)
                    {
                        table.Cell().Padding(2).Text(dia.Data.ToString("dd/MM/yyyy"));
                        table.Cell().Padding(2).Text(dia.Ent1);
                        table.Cell().Padding(2).Text(dia.Sai1);
                        table.Cell().Padding(2).Text(dia.Ent2);
                        table.Cell().Padding(2).Text(dia.Sai2);
                    }
                });

                page.Footer().PaddingTop(2, Unit.Centimetre).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Text("__________________________________\nAssinatura do Trabalhador");
                        row.RelativeItem().AlignCenter().Text("__________________________________\nAssinatura da Empresa");
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}