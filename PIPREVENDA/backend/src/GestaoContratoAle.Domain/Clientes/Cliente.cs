using PortalAle.Domain.Base;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Domain.Clientes;

/// <summary>
/// Cadastro de cliente (pessoa jurídica). Aggregate Root — ver DT-018/DT-022.
/// </summary>
public class Cliente : Entity
{
    public string Nome { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Endereco { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public DateTimeOffset DataCriacao { get; private set; }

    public DateTimeOffset? DataAlteracao { get; private set; }

    public DateTimeOffset? DataExclusao { get; private set; }

    public Cliente()
    {
    }

    public Cliente(int id)
        : base(id)
    {
    }

    public Cliente(string nome, string cnpj, string endereco)
    {
        ValidarNome(nome);
        ValidarCnpj(cnpj);
        ValidarEndereco(endereco);

        Nome = nome.Trim();
        Cnpj = SomenteDigitos(cnpj);
        Endereco = endereco.Trim();
        Ativo = true;
        DataCriacao = DateTimeOffset.UtcNow;
    }

    public void Atualizar(string nome, string cnpj, string endereco, bool ativo)
    {
        GarantirNaoExcluido();

        ValidarNome(nome);
        ValidarCnpj(cnpj);
        ValidarEndereco(endereco);

        Nome = nome.Trim();
        Cnpj = SomenteDigitos(cnpj);
        Endereco = endereco.Trim();
        Ativo = ativo;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void ExcluirLogicamente()
    {
        GarantirNaoExcluido();

        Ativo = false;
        DataExclusao = DateTimeOffset.UtcNow;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    private void GarantirNaoExcluido()
    {
        if (DataExclusao is not null)
            throw new DomainException("Cliente já foi excluído.");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome: O campo Nome é obrigatório");

        if (nome.Trim().Length > 200)
            throw new DomainException("Nome: O campo Nome deve ter no máximo 200 caracteres");
    }

    private static void ValidarEndereco(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainException("Endereco: O campo Endereço é obrigatório");

        if (endereco.Trim().Length > 200)
            throw new DomainException("Endereco: O campo Endereço deve ter no máximo 200 caracteres");
    }

    private static void ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainException("Cnpj: O campo CNPJ é obrigatório");

        if (!EhCnpjValido(cnpj))
            throw new DomainException("Cnpj: CNPJ inválido");
    }

    private static string SomenteDigitos(string valor) => new([.. valor.Where(char.IsDigit)]);

    private static bool EhCnpjValido(string cnpj)
    {
        string digitos = SomenteDigitos(cnpj);

        if (digitos.Length != 14)
            return false;

        if (digitos.Distinct().Count() == 1)
            return false;

        int[] multiplicadoresDigito1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicadoresDigito2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        string base12 = digitos[..12];
        int digito1 = CalcularDigitoVerificador(base12, multiplicadoresDigito1);

        string base13 = base12 + digito1;
        int digito2 = CalcularDigitoVerificador(base13, multiplicadoresDigito2);

        return digitos == base13 + digito2;
    }

    private static int CalcularDigitoVerificador(string baseNumerica, int[] multiplicadores)
    {
        int soma = 0;

        for (int i = 0; i < multiplicadores.Length; i++)
            soma += (baseNumerica[i] - '0') * multiplicadores[i];

        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }
}
