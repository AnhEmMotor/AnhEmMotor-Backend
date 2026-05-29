using Domain.Constants.Permission.Permissions;
using FluentAssertions;
using Infrastructure.Authorization.Attribute;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class SalesOrderSplitEndpoints
{
    [Fact(DisplayName = "SO_083A - SalesOrders exposes split confirmed and unconfirmed list endpoints")]
    public void SalesOrdersController_ShouldExposeSplitListEndpointsWithNewPermissions()
    {
        var viewConfirmedPermission = typeof(Outputs).GetField("ViewConfirmed")?.GetRawConstantValue() as string;
        var viewUnconfirmedPermission = typeof(Outputs).GetField("ViewUnconfirmed")?.GetRawConstantValue() as string;
        viewConfirmedPermission.Should().Be("Permissions.Outputs.ViewConfirmed");
        viewUnconfirmedPermission.Should().Be("Permissions.Outputs.ViewUnconfirmed");

        var confirmedMethod = typeof(SalesOrdersController).GetMethod("GetConfirmedOutputsAsync");
        var unconfirmedMethod = typeof(SalesOrdersController).GetMethod("GetUnconfirmedOutputsAsync");
        confirmedMethod.Should().NotBeNull();
        unconfirmedMethod.Should().NotBeNull();

        confirmedMethod!.GetCustomAttribute<HttpGetAttribute>()?.Template.Should().Be("confirmed");
        unconfirmedMethod!.GetCustomAttribute<HttpGetAttribute>()?.Template.Should().Be("unconfirmed");
        confirmedMethod.GetCustomAttribute<HasPermissionAttribute>()?.Policy
            .Should().Be($"HasPermission{viewConfirmedPermission}");
        unconfirmedMethod.GetCustomAttribute<HasPermissionAttribute>()?.Policy
            .Should().Be($"HasPermission{viewUnconfirmedPermission}");
    }
}
