using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Statistical.Responses;

public record ContractOverviewResponse(
    ContractOverviewKpi Kpi,
    List<ContractTrendData> TrendData,
    List<ContractStatusData> StatusData,
    List<ContractTopSupplierData> TopSuppliersData,
    List<ContractListItem> ContractsData
);

public record ContractOverviewKpi(
    int TotalSalesCount,
    decimal TotalSalesValue,
    int TotalSupplierCount,
    decimal TotalSupplierValue
);

public record ContractTrendData(
    string Day,
    decimal SalesValue,
    decimal SupplierValue
);

public record ContractStatusData(
    string Name,
    int Value
);

public record ContractTopSupplierData(
    string Name,
    decimal Value
);

public record ContractListItem(
    string Id,
    string ContractNumber,
    string Type, // "Bán xe" or "Nhà cung cấp"
    string PartnerName, // Customer or Supplier Name
    decimal Value,
    string Status,
    string Date
);
