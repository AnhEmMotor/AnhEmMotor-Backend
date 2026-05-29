using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.InventoryReceipts.Commands.CloneInput;
using Application.Features.InventoryReceipts.Commands.CreateInput;
using Application.Features.InventoryReceipts.Commands.DeleteInput;
using Application.Features.InventoryReceipts.Commands.DeleteManyInputs;
using Application.Features.InventoryReceipts.Commands.RestoreInput;
using Application.Features.InventoryReceipts.Commands.RestoreManyInputs;
using Application.Features.InventoryReceipts.Commands.UpdateInput;
using Application.Features.InventoryReceipts.Commands.UpdateInputStatus;
using Application.Features.InventoryReceipts.Queries.GetDeletedInputsList;
using Application.Features.InventoryReceipts.Queries.GetInputById;
using Application.Features.InventoryReceipts.Queries.GetInputsBySupplierId;
using Application.Features.InventoryReceipts.Queries.GetInputsList;
using Application.Features.InventoryReceipts.Queries.GetInputStatusList;
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

