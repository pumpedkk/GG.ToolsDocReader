# Decisoes

Registre decisoes importantes deste projeto.

## Entradas

- 2026-03-31: Adicionado `DocWriter.cs` no Runtime para exportar `.csv` (com delimitador configuravel) e `.xlsx` sem dependencias externas.
- 2026-03-31: Definido suporte de conversao bidirecional entre formatos via `ConvertCsvToXlsx` e `ConvertXlsxToCsv`.
- 2026-03-31: Criada classe serializavel `Table` (`Table` + `TableRow`) para facilitar edicao no Inspector da Unity.
- 2026-03-31: `DocWriter` passou a aceitar tanto `IList<IList<string>>` quanto `Table` nas sobrecargas de exportacao.
- 2026-03-31: Documentacao tecnica consolidada em `Runtime/DocWriter.md`.
- 2026-03-31: Exportacao de CSV/XLSX passou a resolver caminho automaticamente com `PrepareOutputPath` (pasta vira `export.csv`/`export.xlsx`, extensao e diretorio sao ajustados).

