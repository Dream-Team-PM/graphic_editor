// graphic_editor.Tests/ViewModels/ColorViewModelTests.cs
using System.Drawing;
using FluentAssertions;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.ViewModels;

public class ColorViewModelTests
{
    [Fact]
    public void Constructor_ShouldSetColor()
    {
        var color = Color.FromArgb(255, 100, 150, 200);
        var vm = new ColorViewModel(color);
        vm.Color.Should().Be(color);
    }

    [Fact]
    public void HexColor_ShouldReturnCorrectHex()
    {
        var vm = new ColorViewModel(Color.FromArgb(255, 10, 20, 30));
        vm.HexColor.Should().Be("#0A141E");
    }

    [Fact]
    public void HexColor_SetWithValidHex_ShouldUpdateColor()
    {
        var vm = new ColorViewModel();
        vm.HexColor = "#FFAABB";
        vm.Color.Should().Be(Color.FromArgb(255, 0xFF, 0xAA, 0xBB));
    }

    [Fact]
    public void HexColor_SetWithInvalidHex_ShouldNotChangeColor()
    {
        var vm = new ColorViewModel(Color.Black);
        vm.HexColor = "invalid";
        vm.Color.Should().Be(Color.Black);
    }

    [Fact]
    public void R_G_B_A_Properties_ShouldUpdateColor()
    {
        var vm = new ColorViewModel(Color.FromArgb(100, 50, 60, 70));
        vm.R = 80;
        vm.G = 90;
        vm.B = 100;
        vm.A = 200;
        vm.Color.Should().Be(Color.FromArgb(200, 80, 90, 100));
    }
}