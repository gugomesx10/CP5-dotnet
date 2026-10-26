using CP5.Application.Results;

namespace CP5.UnitTests.Results;

public class OperationResultTests
{
    [Fact]
    public void Success_DeveRetornarDadosESucesso()
    {
        var result = OperationResult<string>.Success(
            "FURIA",
            "Time encontrado.");

        Assert.True(result.IsSuccess);
        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.Equal("FURIA", result.Data);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void Failure_DeveRetornarErroSemDados()
    {
        var result = OperationResult<string>.Failure(
            OperationStatus.NotFound,
            "TIME_NOT_FOUND",
            "Time não encontrado.");

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal("TIME_NOT_FOUND", result.ErrorCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Failure_NaoDeveAceitarStatusSuccess()
    {
        Assert.Throws<ArgumentException>(() =>
            OperationResult<string>.Failure(
                OperationStatus.Success,
                "INVALID",
                "Erro inválido."));
    }
}