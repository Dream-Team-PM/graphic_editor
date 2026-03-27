// graphic_editor.Tests/ViewModels/HistoryViewModelTests.cs
using System;
using FluentAssertions;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using Moq;
using Xunit;

namespace graphic_editor.Tests.ViewModels;

public class HistoryViewModelTests
{
    [Fact]
    public void AddAction_ShouldAddAndUpdateIndices()
    {
        var history = new HistoryViewModel();
        var mockAction = new Mock<IHistoryAction>();
        mockAction.Setup(a => a.Description).Returns("Test");

        history.AddAction(mockAction.Object);
        history.Actions.Should().Contain(mockAction.Object);
        history.CanUndo.Should().BeTrue();
        history.CanRedo.Should().BeFalse();
        history.CurrentActionDescription.Should().Be("Test");
    }

    [Fact]
    public void Undo_ShouldCallUndoOnCurrentActionAndMoveBack()
    {
        var history = new HistoryViewModel();
        var mockAction1 = new Mock<IHistoryAction>();
        var mockAction2 = new Mock<IHistoryAction>();
        mockAction1.Setup(a => a.Description).Returns("Action1");
        mockAction2.Setup(a => a.Description).Returns("Action2");

        history.AddAction(mockAction1.Object);
        history.AddAction(mockAction2.Object);

        history.Undo();
        mockAction2.Verify(a => a.Undo(), Times.Once);
        history.CanUndo.Should().BeTrue();
        history.CanRedo.Should().BeTrue();
        history.CurrentActionDescription.Should().Be("Action1");
    }

    [Fact]
    public void Redo_ShouldCallRedoOnNextAction()
    {
        var history = new HistoryViewModel();
        var mockAction1 = new Mock<IHistoryAction>();
        var mockAction2 = new Mock<IHistoryAction>();
        mockAction1.Setup(a => a.Description).Returns("Action1");
        mockAction2.Setup(a => a.Description).Returns("Action2");

        history.AddAction(mockAction1.Object);
        history.AddAction(mockAction2.Object);
        history.Undo();
        history.Redo();
        mockAction2.Verify(a => a.Redo(), Times.Once);
        history.CanUndo.Should().BeTrue();
        history.CanRedo.Should().BeFalse();
        history.CurrentActionDescription.Should().Be("Action2");
    }

    [Fact]
    public void Clear_ShouldRemoveAllActions()
    {
        var history = new HistoryViewModel();
        var mockAction = new Mock<IHistoryAction>();
        mockAction.Setup(a => a.Description).Returns("Test");
        history.AddAction(mockAction.Object);
        history.Clear();
        history.Actions.Should().BeEmpty();
        history.CanUndo.Should().BeFalse();
        history.CanRedo.Should().BeFalse();
    }
}