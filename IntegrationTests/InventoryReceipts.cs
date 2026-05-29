using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Features.InventoryReceipts.Commands.CreateInput;
using Application.Features.InventoryReceipts.Commands.DeleteManyInputs;
using Application.Features.InventoryReceipts.Commands.RestoreManyInputs;
using Application.Features.InventoryReceipts.Commands.UpdateInput;
using Application.Features.InventoryReceipts.Commands.UpdateInputNotes;
using Application.Features.InventoryReceipts.Commands.UpdateInputStatus;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrandEntity = Domain.Entities.Brand;
using InputEntity = Domain.Entities.InventoryReceipt;
using InputInfoEntity = Domain.Entities.InputInfo;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using SupplierEntity = Domain.Entities.Supplier;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;
using ProductStatusConstants = Domain.Constants.Product.ProductStatus;

public class InventoryReceipts : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public InventoryReceipts(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035

    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}

