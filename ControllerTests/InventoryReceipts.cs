using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Features.Inputs.Commands.CloneInput;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreInput;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Queries.GetDeletedInputsList;
using Application.Features.Inputs.Queries.GetInputById;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Inputs.Queries.GetInputsList;
using Application.Features.Inputs.Queries.GetInputStatusList;
using Domain.Constants.Input;
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

