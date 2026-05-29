using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.DeleteInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;
using Application.Features.InventoryReceipts.Commands.RestoreInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.RestoreManyInventoryReceipts;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;
using Application.Features.InventoryReceipts.Queries.GetDeletedInventoryReceiptsList;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsList;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStatusList;
using Domain.Constants.InventoryReceipt;
using Domain.Primitives;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class InventoryReceipts
{
    public InventoryReceipts()
    {
        
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}

