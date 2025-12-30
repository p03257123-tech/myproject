using WGCCI.Accounting.Api.Data;

namespace WGCCI.Accounting.Api.DTOs;

public record AccountCreateDto(
    string Code,
    string Name,
    AccountType Type,
    string? Currency
);

public record JournalLineDto(
    string AccountCode,
    decimal Debit,
    decimal Credit
);

public record JournalEntryDto(
    DateOnly Date,
    string? Memo,
    IEnumerable<JournalLineDto> Lines
);

public record BudgetDto(
    string AccountCode,
    string Period,
    decimal Amount
);

public record ForecastDto(
    string AccountCode,
    string Period,
    decimal Amount,
    string Method
);

public record RateDto(
    DateOnly Date,
    string FromCurrency,
    string ToCurrency,
    decimal Rate
);

public record TaxCodeDto(
    string Name,
    decimal Rate,
    TaxType Type
);

public record BankRuleDto(
    string Pattern,
    string AccountCode
);
