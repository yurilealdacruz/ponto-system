using Ponto.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class RelatorioPdfService
{
    public byte[] GerarRelatorioMensal(string nomeFuncionario, string mesAno, List<RegistroPonto> registros)
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
                    col.Item().Text("Registo de Tempos de Trabalho").SemiBold().FontSize(16).FontColor(Colors.BlueDarken2);
                    col.Item().Text($"Colaborador: {nomeFuncionario}");
                    col.Item().Text($"Mês/Ano: {mesAno}");
                    col.Item().PaddingBottom(1, Unit.Centimetre);
                });

                // Tabela de Pontos
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(); // Data
                        columns.RelativeColumn(); // Ent 1
                        columns.RelativeColumn(); // Sai 1
                        columns.RelativeColumn(); // Ent 2
                        columns.RelativeColumn(); // Sai 2
                    });

                    // Cabeçalho da Tabela
                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Padding(2).Text("Data").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Ent 1").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Sai 1").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Ent 2").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Sai 2").SemiBold();
                    });

                    // Linhas da Tabela
                    foreach (var reg in registros)
                    {
                        table.Cell().Padding(2).Text(reg.Data.ToString("dd/MM/yyyy"));
                        table.Cell().Padding(2).Text(reg.Entrada1?.ToString(@"hh\:mm") ?? "-");
                        table.Cell().Padding(2).Text(reg.Saida1?.ToString(@"hh\:mm") ?? "-");
                        table.Cell().Padding(2).Text(reg.Entrada2?.ToString(@"hh\:mm") ?? "-");
                        table.Cell().Padding(2).Text(reg.Saida2?.ToString(@"hh\:mm") ?? "-");
                    }
                });

                // Rodapé com Assinaturas
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