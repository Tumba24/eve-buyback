using System.Globalization;
using MediatR;

namespace EveBuyback.App;

public record ContractQuery(string RawInput) : IRequest<ContractQueryResult>;

public record ContractQueryItem(string ItemTypeName, long Volume);

public record ContractQueryResult(IEnumerable<ContractQueryItem>? Items, bool OK, string ErrorMessage);

internal class ContractQueryHandler : IRequestHandler<ContractQuery, ContractQueryResult>
{
    public async Task<ContractQueryResult> Handle(ContractQuery query, CancellationToken token)
    {
        var items = new List<ContractQueryItem>();

        using (StringReader reader = new StringReader(query.RawInput))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                token.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(line))
                    continue;
                
                line = line.Trim();
                
                var parts = line.Split(new string[] { "\t", "  " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    return new ContractQueryResult(
                        null, 
                        false, 
                        "Each line should split into two parts. Parts should be split by a tab or two spaces.");
                }

                if (!Int64.TryParse(parts[1].Trim(), NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var volume))
                {
                    return new ContractQueryResult(
                        null,
                        false,
                        "The second part of each line must be a valid 64 bit integer that indicates volume.");
                }

                var itemTypeName = SanitizeItemTypeName(parts[0]);
                
                items.Add(new ContractQueryItem(itemTypeName, volume));
            }
        }

        return new ContractQueryResult(items, true, string.Empty);
    }

    private string SanitizeItemTypeName(string rawItemTypeName)
    {
        var itemTypeName = rawItemTypeName.Trim();

        itemTypeName  = SanitizeEveTranslationAsteriskSuffix(itemTypeName);

        return itemTypeName.Trim();

        string SanitizeEveTranslationAsteriskSuffix(string input)
        {
            if (input.EndsWith('*'))
                input = input.Substring(0, input.Length - 1);

            return input;
        }
    }
}