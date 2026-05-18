using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.ValueObjects.Users;
using ArchLens.SharedKernel.Application;
using ArchLens.SharedKernel.Domain;
using FluentAssertions;

namespace ArchLens.Auth.Tests.SharedKernel;

// ─── Entity tests (via User which extends Entity<Guid>) ──────────────────────

public class EntityTests
{
    [Fact]
    public void Equals_SameReference_ShouldBeEqual()
    {
        var user = User.Create("user1", "user1@test.com", "hash", DateTime.UtcNow);
        var sameRef = user;

        user.Equals(sameRef).Should().BeTrue();
        (user == sameRef).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ShouldNotBeEqual()
    {
        var user1 = User.Create("user1", "user1@test.com", "hash1", DateTime.UtcNow);
        var user2 = User.Create("user2", "user2@test.com", "hash2", DateTime.UtcNow);

        user1.Equals(user2).Should().BeFalse();
        (user1 != user2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        user.Equals(null).Should().BeFalse();
        (user == null).Should().BeFalse();
        (null == user).Should().BeFalse();
    }

    [Fact]
    public void Equals_BothNull_ShouldReturnTrue()
    {
        User? a = null;
        User? b = null;

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_SameReference_ShouldBeEqual()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);
        object obj = user;

        user.Equals(obj).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_DifferentType_ShouldReturnFalse()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        user.Equals("not-an-entity").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameEntity_ShouldBeSame()
    {
        var user = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        user.GetHashCode().Should().Be(user.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentEntities_ShouldBeDifferent()
    {
        var user1 = User.Create("user1", "user1@test.com", "hash1", DateTime.UtcNow);
        var user2 = User.Create("user2", "user2@test.com", "hash2", DateTime.UtcNow);

        user1.GetHashCode().Should().NotBe(user2.GetHashCode());
    }

    [Fact]
    public void NotEquals_Operator_DifferentIds_ShouldBeTrue()
    {
        var u1 = User.Create("u1", "u1@test.com", "h1", DateTime.UtcNow);
        var u2 = User.Create("u2", "u2@test.com", "h2", DateTime.UtcNow);

        (u1 != u2).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullLeft_ShouldReturnFalse()
    {
        User? left = null;
        var right = User.Create("user", "user@test.com", "hash", DateTime.UtcNow);

        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }
}

// ─── ValueObject tests (via Email which extends ValueObject) ──────────────────

public class ValueObjectTests
{
    [Fact]
    public void Equals_SameValues_ShouldBeEqual()
    {
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("USER@EXAMPLE.COM");

        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ShouldNotBeEqual()
    {
        var email1 = Email.Create("alice@example.com");
        var email2 = Email.Create("bob@example.com");

        email1.Equals(email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var email = Email.Create("user@example.com");

        email.Equals(null).Should().BeFalse();
        (email == null).Should().BeFalse();
        (null == email).Should().BeFalse();
    }

    [Fact]
    public void Equals_BothNull_ShouldReturnTrue()
    {
        Email? a = null;
        Email? b = null;

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObjectOfDifferentType_ShouldReturnFalse()
    {
        var email = Email.Create("user@example.com");

        email.Equals("user@example.com").Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObjectOfSameValue_ShouldBeTrue()
    {
        var email = Email.Create("user@example.com");
        object obj = Email.Create("user@example.com");

        email.Equals(obj).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValues_ShouldBeSame()
    {
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("user@example.com");

        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldBeDifferent()
    {
        var email1 = Email.Create("alice@example.com");
        var email2 = Email.Create("bob@example.com");

        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }

    [Fact]
    public void NotEquals_Operator_ShouldWork()
    {
        var email1 = Email.Create("alice@example.com");
        var email2 = Email.Create("bob@example.com");

        (email1 != email2).Should().BeTrue();
        (email1 != email1).Should().BeFalse();
    }
}

// ─── Result tests ─────────────────────────────────────────────────────────────

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        var error = new Error("TEST", "Test error");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_Generic_ShouldContainValue()
    {
        var result = Result.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_Generic_ShouldThrowOnValueAccess()
    {
        var result = Result.Failure<string>(Error.NotFound);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSuccessResult()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_Generic_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.Validation);

        var act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    [Fact]
    public void Error_Predefined_ShouldHaveCorrectValues()
    {
        Error.None.Code.Should().BeEmpty();
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NotFound.Code.Should().Be("Error.NotFound");
        Error.Conflict.Code.Should().Be("Error.Conflict");
        Error.Validation.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public void Error_Equality_ShouldWork()
    {
        var error1 = new Error("CODE", "Desc");
        var error2 = new Error("CODE", "Desc");

        error1.Should().Be(error2);
    }

    [Fact]
    public void Success_Generic_IsFailure_ShouldBeFalse()
    {
        var result = Result.Success(42);

        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_Error_ShouldNotBeNone()
    {
        var result = Result.Failure(Error.Conflict);

        result.Error.Should().NotBe(Error.None);
    }
}

// ─── PagedRequest tests ───────────────────────────────────────────────────────

public class PagedRequestTests
{
    [Fact]
    public void Default_ShouldHavePage1And20PageSize()
    {
        var request = new PagedRequest();

        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
        request.Skip.Should().Be(0);
    }

    [Fact]
    public void Page2_ShouldSkip20()
    {
        var request = new PagedRequest(2, 20);

        request.Skip.Should().Be(20);
    }

    [Fact]
    public void Page3_PageSize10_ShouldSkip20()
    {
        var request = new PagedRequest(3, 10);

        request.Skip.Should().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PageLessThan1_ShouldDefaultTo1(int page)
    {
        var request = new PagedRequest(page, 20);

        request.Page.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageSizeLessThan1_ShouldDefaultTo20(int pageSize)
    {
        var request = new PagedRequest(1, pageSize);

        request.PageSize.Should().Be(20);
    }

    [Fact]
    public void PageSizeOver100_ShouldClampTo100()
    {
        var request = new PagedRequest(1, 200);

        request.PageSize.Should().Be(100);
    }

    [Fact]
    public void PageSize100_ShouldBeAllowed()
    {
        var request = new PagedRequest(1, 100);

        request.PageSize.Should().Be(100);
    }
}

// ─── PagedResponse tests ──────────────────────────────────────────────────────

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        var response = new PagedResponse<string>(new List<string>(), 1, 10, 25);

        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasPrevious_Page1_ShouldBeFalse()
    {
        var response = new PagedResponse<string>(new List<string>(), 1, 10, 50);

        response.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_Page2_ShouldBeTrue()
    {
        var response = new PagedResponse<string>(new List<string>(), 2, 10, 50);

        response.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_LastPage_ShouldBeFalse()
    {
        var response = new PagedResponse<string>(new List<string>(), 5, 10, 50);

        response.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_NotLastPage_ShouldBeTrue()
    {
        var response = new PagedResponse<string>(new List<string>(), 1, 10, 50);

        response.HasNext.Should().BeTrue();
    }

    [Fact]
    public void TotalPages_ZeroItems_ShouldBeZero()
    {
        var response = new PagedResponse<string>(new List<string>(), 1, 10, 0);

        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public void Items_ShouldReturnCorrectItems()
    {
        var items = new List<string> { "a", "b", "c" };
        var response = new PagedResponse<string>(items, 1, 10, 3);

        response.Items.Should().BeEquivalentTo(items);
        response.TotalCount.Should().Be(3);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }
}

// ─── DomainException tests ────────────────────────────────────────────────────

public class DomainExceptionTests
{
    private sealed class TestDomainException : DomainException
    {
        public TestDomainException(string code, string message) : base(code, message) { }
    }

    private sealed class TestDomainInnerException : DomainException
    {
        public TestDomainInnerException(string code, string message, Exception inner)
            : base(code, message, inner) { }
    }

    [Fact]
    public void DomainException_WithInnerException_ShouldPreserveInner()
    {
        var inner = new Exception("inner error");
        var ex = new TestDomainInnerException("ERR001", "Something went wrong", inner);

        ex.Code.Should().Be("ERR001");
        ex.InnerException.Should().Be(inner);
        ex.Message.Should().Contain("Something went wrong");
    }

    [Fact]
    public void DomainException_WithoutInnerException_ShouldWork()
    {
        var ex = new TestDomainException("ERR001", "Domain error");

        ex.Code.Should().Be("ERR001");
        ex.Message.Should().Contain("Domain error");
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void DomainException_ShouldBeAssignableTo_Exception()
    {
        var ex = new TestDomainException("CODE", "message");

        ex.Should().BeAssignableTo<Exception>();
    }
}
