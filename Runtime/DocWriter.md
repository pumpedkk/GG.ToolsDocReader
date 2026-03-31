# DocWriter - Guia de Entendimento

Este documento explica o `DocWriter.cs` de forma prática: o que ele faz, como usar e como ele funciona internamente.

## Objetivo

O `DocWriter` foi criado para:

- Exportar dados tabulares para `.csv` com divisor configurável.
- Exportar dados tabulares para `.xlsx` (planilha Excel).
- Converter arquivos `.csv` para `.xlsx`.
- Converter arquivos `.xlsx` para `.csv`.
- Funcionar tanto com `IList<IList<string>>` quanto com a classe `Table` (editável no Inspector).

Tudo foi implementado sem dependências externas, usando apenas bibliotecas padrão do .NET/Unity.

## Estrutura geral

Classe:

- `public static class DocWriter`

Isso significa:

- Não precisa instanciar objeto (`new`).
- Todos os métodos são utilitários estáticos.

## Métodos públicos

Atualmente existem 6 métodos públicos no `DocWriter`:

## Regras de caminho de saída (importante)

Nos métodos de exportação (`ExportCsv` e `ExportXlsx`), o caminho é tratado automaticamente:

- Se você passar uma **pasta**, o `DocWriter` cria arquivo padrão dentro dela:
  - CSV: `export.csv`
  - XLSX: `export.xlsx`
- Se passar caminho sem extensão, a extensão correta é adicionada automaticamente.
- O diretório de saída é criado automaticamente se não existir.

Isso evita erro comum de permissao quando um caminho de pasta e usado como se fosse arquivo.

### 1) `ExportCsv(...)` com `Table`

```csharp
public static void ExportCsv(string filePath, Table table, char delimiter = ',')
```

O que faz:

- Exporta para CSV direto a partir da classe `Table`.
- Ideal quando os dados foram montados no Inspector.
- Resolve automaticamente o caminho final de saida (arquivo/pasta).

---

### 2) `ExportCsv(...)` com `IList`

```csharp
public static void ExportCsv(string filePath, IList<IList<string>> table, char delimiter = ',')
```

O que faz:

- Recebe uma tabela em memória (`linhas x colunas`).
- Gera conteúdo CSV.
- Escapa campos com aspas/quebra de linha/divisor.
- Salva em disco com UTF-8 BOM.
- Resolve automaticamente o caminho final de saida (arquivo/pasta).

Quando usar:

- Quando você já tem os dados na memória e quer gerar um arquivo CSV.

---

### 3) `ExportXlsx(...)` com `IList`

```csharp
public static void ExportXlsx(string filePath, IList<IList<string>> table, string sheetName = "Sheet1")
```

O que faz:

- Recebe tabela em memória.
- Monta um arquivo `.xlsx` válido usando OpenXML dentro de um `.zip`.
- Cria estrutura mínima necessária:
  - `[Content_Types].xml`
  - `_rels/.rels`
  - `xl/workbook.xml`
  - `xl/_rels/workbook.xml.rels`
  - `xl/styles.xml`
  - `xl/worksheets/sheet1.xml`
- Resolve automaticamente o caminho final de saida (arquivo/pasta).

Quando usar:

- Quando quer gerar Excel diretamente a partir dos dados.

---

### 4) `ExportXlsx(...)` com `Table`

```csharp
public static void ExportXlsx(string filePath, Table table, string sheetName = "Sheet1")
```

O que faz:

- Exporta para XLSX direto a partir da classe `Table`.
- Ideal quando os dados foram montados no Inspector.
- Resolve automaticamente o caminho final de saida (arquivo/pasta).

---

### 5) `ConvertCsvToXlsx(...)`

```csharp
public static void ConvertCsvToXlsx(string csvFilePath, string xlsxFilePath, char delimiter = ',', string sheetName = "Sheet1")
```

O que faz:

- Lê CSV do disco.
- Faz parse respeitando aspas e delimitador configurável.
- Reusa `ExportXlsx` para salvar como Excel.

Fluxo:

`CSV (texto) -> ParseCsv -> tabela -> ExportXlsx -> XLSX`

---

### 6) `ConvertXlsxToCsv(...)`

```csharp
public static void ConvertXlsxToCsv(string xlsxFilePath, string csvFilePath, char delimiter = ',')
```

O que faz:

- Abre XLSX (zip).
- Lê a primeira worksheet.
- Extrai valores das células.
- Reusa `ExportCsv` para salvar em CSV.

Fluxo:

`XLSX -> ReadFirstWorksheet -> tabela -> ExportCsv -> CSV`

## Classe `Table` (Inspector Friendly)

Para facilitar uso por designers e não-programadores, foi criada a classe serializável `Table` em `Table.cs`.

Estrutura:

- `Table.Rows` -> `List<TableRow>`
- `TableRow.Columns` -> `List<string>`

Como ela ajuda:

- Pode ser editada diretamente no Inspector da Unity.
- Evita montar manualmente `List<IList<string>>` para casos simples.
- Possui conversão para o formato do `DocWriter` com `ToIList()`.

## Sobre `IList<IList<string>>`

Esse tipo representa uma tabela:

- `IList<...>` externo = linhas.
- `IList<string>` interno = colunas de cada linha.

Exemplo:

```csharp
var table = new List<IList<string>>
{
    new List<string> { "Nome", "Idade", "Cidade" },
    new List<string> { "Ana", "29", "Sao Paulo" },
    new List<string> { "Carlos", "35", "Rio de Janeiro" }
};
```

## Funções internas importantes

### `EscapeCsv(...)`

- Garante CSV válido:
  - Se houver delimitador, aspas ou quebra de linha, envolve campo com `"..."`.
  - Aspas internas viram `""`.

### `ParseCsv(...)`

- Parser manual caractere a caractere.
- Suporta:
  - delimitador customizado (`;`, `,`, etc.)
  - campos com aspas
  - aspas escapadas (`""`)
  - quebra de linha em campo entre aspas

### `ReadFirstWorksheet(...)`

- Lê o `.xlsx` como zip.
- Encontra a planilha (`sheet1.xml` ou primeira disponível).
- Lê células e recompõe linhas/colunas.
- Trata células:
  - `inlineStr`
  - `shared strings` (`t="s"`)
  - `v` direto (números/texto bruto)

### `GetColumnName(...)` e `GetCellReference(...)`

- Convertem índice para referência Excel:
  - `0 -> A`, `1 -> B`, `25 -> Z`, `26 -> AA`
  - Exemplo de célula: `B3`

## Exemplos de uso no Unity

### Usando `Table` no Inspector e exportando CSV/XLSX

```csharp
using GGTools.FileReaders;
using UnityEngine;

public class TableInspectorExporter : MonoBehaviour
{
    public Table TableData = new Table();
    public char Delimiter = ';';
    public string SheetName = "Planilha";

    [ContextMenu("Export CSV")]
    private void ExportCsv()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "inspector_data.csv");
        DocWriter.ExportCsv(path, TableData, Delimiter);
    }

    [ContextMenu("Export XLSX")]
    private void ExportXlsx()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "inspector_data.xlsx");
        DocWriter.ExportXlsx(path, TableData, SheetName);
    }
}
```

### Exportar CSV com `;`

```csharp
using GGTools.FileReaders;
using System.Collections.Generic;
using UnityEngine;

public class CsvExample : MonoBehaviour
{
    void Start()
    {
        var table = new List<IList<string>>
        {
            new List<string> { "Produto", "Preco" },
            new List<string> { "Caneta", "2.50" },
            new List<string> { "Caderno", "15.90" }
        };

        string path = System.IO.Path.Combine(Application.persistentDataPath, "produtos.csv");
        DocWriter.ExportCsv(path, table, ';');
    }
}
```

### Passando pasta em vez de arquivo (comportamento automatico)

```csharp
using GGTools.FileReaders;
using System.Collections.Generic;
using UnityEngine;

public class FolderPathExample : MonoBehaviour
{
    void Start()
    {
        var table = new List<IList<string>>
        {
            new List<string> { "A", "B" },
            new List<string> { "1", "2" }
        };

        string folderPath = System.IO.Path.Combine(Application.persistentDataPath, "Exports");

        // Gera: .../Exports/export.csv
        DocWriter.ExportCsv(folderPath, table, ';');

        // Gera: .../Exports/export.xlsx
        DocWriter.ExportXlsx(folderPath, table, "Dados");
    }
}
```

### Converter CSV para XLSX

```csharp
using GGTools.FileReaders;
using UnityEngine;

public class ConvertCsvToXlsxExample : MonoBehaviour
{
    void Start()
    {
        string csvPath = System.IO.Path.Combine(Application.persistentDataPath, "dados.csv");
        string xlsxPath = System.IO.Path.Combine(Application.persistentDataPath, "dados.xlsx");

        DocWriter.ConvertCsvToXlsx(csvPath, xlsxPath, ';', "Relatorio");
    }
}
```

### Converter XLSX para CSV

```csharp
using GGTools.FileReaders;
using UnityEngine;

public class ConvertXlsxToCsvExample : MonoBehaviour
{
    void Start()
    {
        string xlsxPath = System.IO.Path.Combine(Application.persistentDataPath, "dados.xlsx");
        string csvPath = System.IO.Path.Combine(Application.persistentDataPath, "dados_exportados.csv");

        DocWriter.ConvertXlsxToCsv(xlsxPath, csvPath, ',');
    }
}
```

## Limitações atuais (importante)

- O export para XLSX grava apenas uma planilha.
- Não aplica formatação visual avançada (cores, bordas customizadas, formulas, etc.).
- A leitura de XLSX considera a primeira planilha encontrada.
- Datas/números são tratados como texto cru em muitos cenários (intencional para evitar perda de dados).

## Melhorias futuras sugeridas

- Suporte a múltiplas planilhas.
- Suporte a tipo de célula mais rico (número, data, bool).
- API com `string[][]` para ficar alinhada ao `DocReader.SplitCSV`.
- Opção de incluir/omitir BOM no CSV.
- Suporte a streaming para arquivos muito grandes.

## Resumo rápido

Se você lembrar apenas disso:

- `ExportCsv` e `ExportXlsx`: escrevem a tabela para arquivo (aceitam `IList` e `Table`).
- `ConvertCsvToXlsx` e `ConvertXlsxToCsv`: convertem entre formatos.
- `delimiter` permite usar `,` ou `;` no CSV.
- `IList<IList<string>>` representa linhas e colunas da tabela.
- `Table` foi criada para facilitar edição no Inspector.
- Se voce passar uma pasta no export, o arquivo padrao `export.csv`/`export.xlsx` e criado automaticamente.
