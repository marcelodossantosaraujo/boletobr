﻿using System;
using System.Collections.Generic;
using System.Linq;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Itau
{
    public class EscritorRemessaCnab400Itau : IEscritorArquivoRemessaCnab400
    {
        private RemessaCnab400 _remessaEscrever;

        public EscritorRemessaCnab400Itau(RemessaCnab400 remessaEscrever)
        {
            _remessaEscrever = remessaEscrever;
        }

        public string EscreverHeader(HeaderRemessaCnab400 infoHeader)
        {
            var header = new string(' ', 400);
            try
            {
                header = header.PreencherValorNaLinha(1, 1, "0");
                header = header.PreencherValorNaLinha(2, 2, "1");
                header = header.PreencherValorNaLinha(3, 9, "REMESSA");
                header = header.PreencherValorNaLinha(10, 11, "01");
                header = header.PreencherValorNaLinha(12, 26, "COBRANCA".PadRight(15, ' '));
                header = header.PreencherValorNaLinha(27, 30, infoHeader.Agencia.PadLeft(4, '0'));
                header = header.PreencherValorNaLinha(31, 32, "00");
                header = header.PreencherValorNaLinha(33, 37, infoHeader.ContaCorrente.PadLeft(5, '0'));
                header = header.PreencherValorNaLinha(38, 38, infoHeader.DvContaCorrente);
                header = header.PreencherValorNaLinha(39, 46, string.Empty.PadRight(8, ' '));
                header = header.PreencherValorNaLinha(47, 76, infoHeader.NomeEmpresa.PadRight(30, ' '));
                header = header.PreencherValorNaLinha(77, 79, "341");
                header = header.PreencherValorNaLinha(80, 94, "BANCO ITAU S.A.".PadRight(15, ' '));
                header = header.PreencherValorNaLinha(95, 100, DateTime.Now.ToString("ddMMyy").Replace("/", ""));
                header = header.PreencherValorNaLinha(101, 394, string.Empty.PadRight(294, ' '));
                header = header.PreencherValorNaLinha(395, 400, "000001");

                return header;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("BoletoBr{0}Falha na geração do HEADER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverDetalhe(DetalheRemessaCnab400 infoDetalhe)
        {
            // Na geração do detalhe na remessa não está sendo tratado os casos de cancelamento das instruções nas posições 34-37

            #region Variáveis

            string nossoNumeroCarteira =
                infoDetalhe.NossoNumeroFormatado.Replace(".", "").Replace("/", "").Replace("-", "").Substring(0, 3);
            string nossoNumeroSequencial =
                infoDetalhe.NossoNumeroFormatado.Replace(".", "").Replace("/", "").Replace("-", "").Substring(3, 8);
            string nossoNumeroDigito =
                infoDetalhe.NossoNumeroFormatado.Replace(".", "").Replace("/", "").Replace("-", "").Substring(11, 1);

            string carteiraCob = infoDetalhe.CarteiraCobranca.PadLeft(3, ' ');
            string enderecoSacado = infoDetalhe.EnderecoPagador;
            string bairroSacado = infoDetalhe.BairroPagador;
            string cidadeSacado = infoDetalhe.CidadePagador;

            #endregion

            if (enderecoSacado.Length > 40)
                enderecoSacado.Substring(0, 40);

            if (bairroSacado.Length > 12)
                bairroSacado.Substring(0, 12);

            if (cidadeSacado.Length > 15)
                cidadeSacado.Substring(0, 15);

            var detalhe = new string(' ', 400);
            try
            {
                detalhe = detalhe.PreencherValorNaLinha(1, 1, "1"); // Identificação do Registro Transação
                detalhe = detalhe.PreencherValorNaLinha(2, 3,
                    infoDetalhe.InscricaoCedente.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11
                        ? "01"
                        : "02"); // Tipo de Inscrição da Empresa
                detalhe = detalhe.PreencherValorNaLinha(4, 17,
                    infoDetalhe.InscricaoCedente.Replace(".", "").Replace("/", "").Replace("-", ""));
                    // Nro de Inscrição da Empresa (CPF/CNPJ)
                detalhe = detalhe.PreencherValorNaLinha(18, 21, infoDetalhe.Agencia.PadLeft(4, '0')); // Agência Mantenedora da Conta
                detalhe = detalhe.PreencherValorNaLinha(22, 23, string.Empty.PadRight(2, '0'));
                    // Complemento de Registro
                detalhe = detalhe.PreencherValorNaLinha(24, 28, infoDetalhe.ContaCorrente.PadLeft(5, '0'));
                    // Nro da Conta Corrente da Empresa
                detalhe = detalhe.PreencherValorNaLinha(29, 29, infoDetalhe.DvContaCorrente);
                    // Dígito de Auto Conferência Ag/Conta Empresa
                detalhe = detalhe.PreencherValorNaLinha(30, 33, string.Empty.PadRight(4, ' '));
                    // Complemento de Registro

                if (infoDetalhe.CodigoOcorrencia.Codigo != 35 && infoDetalhe.CodigoOcorrencia.Codigo != 38)
                    detalhe = detalhe.PreencherValorNaLinha(34, 37, "0000"); // Cód. Instrução/Alegação a ser cancelada

                const string doc = "DOC";
                var seuNumero = doc + infoDetalhe.NossoNumeroFormatado.PadRight(25 - doc.Length, ' ');

                detalhe = detalhe.PreencherValorNaLinha(38, 62, seuNumero); // Identificação do Título na Empresa
                detalhe = detalhe.PreencherValorNaLinha(63, 70, nossoNumeroSequencial);
                    // Identificação do Título no Banco

                // Se Moeda = REAL, preenche com zeros
                if (infoDetalhe.Moeda == "9" || infoDetalhe.Moeda == "09" || infoDetalhe.Moeda == "R$" || infoDetalhe.Moeda == "REAL")
                    detalhe = detalhe.PreencherValorNaLinha(71, 83, infoDetalhe.QuantidadeMoeda.ToString().PadLeft(13, '0'));
                        // Quantidade de Moeda Variável
                    // Caso contrário, preenche com a quantidade
                else
                    detalhe = detalhe.PreencherValorNaLinha(71, 83, String.Format("{0:0.#####}", infoDetalhe.QuantidadeMoeda)
                        .Replace(".", "")
                        .Replace(",", "")
                        .PadLeft(13, '0')); // Quantidade de Moeda Variável
                detalhe = detalhe.PreencherValorNaLinha(84, 86, infoDetalhe.CarteiraCobranca.PadLeft(3, '0'));
                    // Número da Carteira no Banco
                detalhe = detalhe.PreencherValorNaLinha(87, 107, string.Empty.PadRight(21, ' '));
                    // Identificação da Operação no Banco
                /* Código da Carteira */
                // Modalidade de Carteira D - Direta
                if (carteiraCob == "108")
                    detalhe = detalhe.PreencherValorNaLinha(108, 108, "D");
                // Modalidade de Carteira S - Sem Registro
                if (carteiraCob == "103" || carteiraCob == "173" || carteiraCob == "196")
                    detalhe = detalhe.PreencherValorNaLinha(108, 108, "S");
                // Modalidade de Carteira E - Escritural
                if (carteiraCob == "104" || carteiraCob == "112" || carteiraCob == "138" || carteiraCob == "147")
                    detalhe = detalhe.PreencherValorNaLinha(108, 108, "E");
                detalhe = detalhe.PreencherValorNaLinha(109, 110,
                    infoDetalhe.CodigoOcorrencia.Codigo.ToString().PadLeft(2, '0')); // Identificação da Ocorrência
                detalhe = detalhe.PreencherValorNaLinha(111, 120,
                    infoDetalhe.NumeroDocumento.Replace("-", "").PadLeft(10, '0')); // Nro do Documento de Cobrança
                detalhe = detalhe.PreencherValorNaLinha(121, 126, infoDetalhe.DataVencimento.ToString("ddMMyy"));
                    // Data de Vencimento do Título
                detalhe = detalhe.PreencherValorNaLinha(127, 139,
                    infoDetalhe.ValorBoleto.ToString("f").Replace(".", "").Replace(",", "").PadLeft(13, '0'));
                    // Valor Nominal do Título
                detalhe = detalhe.PreencherValorNaLinha(140, 142, "341"); // Nro do Banco na Câmara de Compensação
                detalhe = detalhe.PreencherValorNaLinha(143, 147, string.Empty.PadLeft(5, '0'));
                    // Agência onde o título será cobrado
                // Espécie do documento padronizado para DM - Duplicata Mercantil
                detalhe = detalhe.PreencherValorNaLinha(148, 149,
                    infoDetalhe.Especie.Sigla.Equals("DM") ? "01" : infoDetalhe.Especie.Codigo.ToString()); // Espécie do Título
                detalhe = detalhe.PreencherValorNaLinha(150, 150, infoDetalhe.Aceite.Equals("A") ? "A" : "N");
                    // Identificação de Título Aceitou ou Não Aceito
                detalhe = detalhe.PreencherValorNaLinha(151, 156, infoDetalhe.DataEmissao.ToString("ddMMyy"));
                    // Data da Emissão do Título

                #region INSTRUÇÕES REMESSA

                if (infoDetalhe.Instrucoes.Count > 2)
                    throw new Exception(
                        string.Format(
                            "<BoletoBr>{0}Não são aceitas mais que 2 instruções padronizadas para remessa de boletos no banco Itaú.",
                            Environment.NewLine));

                var primeiraInstrucao = infoDetalhe.Instrucoes.FirstOrDefault();
                var segundaInstrucao = infoDetalhe.Instrucoes.LastOrDefault();

                // No caso da instrução "39", se informar "00" na posição 392-393 será impresso no boleto a literal "NÃO RECEBER APÓS O VENCIMENTO".
                if (primeiraInstrucao != null)
                    detalhe = detalhe.PreencherValorNaLinha(157, 158, primeiraInstrucao.ToString());
                else
                    detalhe = detalhe.PreencherValorNaLinha(157, 158, "39");

                if (segundaInstrucao != null)
                    detalhe = detalhe.PreencherValorNaLinha(159, 160, segundaInstrucao.ToString());
                else
                    detalhe = detalhe.PreencherValorNaLinha(159, 160, "39");

                #endregion

                detalhe = detalhe.PreencherValorNaLinha(161, 173,
                    infoDetalhe.ValorMoraDia.ToString("f").Replace(",", "").PadLeft(13, '0'));
                    // Valor de Mora Por Dia de Atraso

                if (infoDetalhe.DataDesconto == DateTime.MinValue)
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, string.Empty.PadLeft(6, '0'));
                else
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, infoDetalhe.DataDesconto.ToString("ddMMyy"));
                        // Data Limite para Concesão de Desconto

                detalhe = detalhe.PreencherValorNaLinha(180, 192,
                    infoDetalhe.ValorDesconto.ToString().Replace(",", "").PadLeft(13, '0'));
                    // Valor do Desconto a ser Concedido
                detalhe = detalhe.PreencherValorNaLinha(193, 205,
                    infoDetalhe.ValorIof.ToString().Replace(",", "").PadLeft(13, '0'));
                    // Valor do I.O.F. recolhido p/ notas seguro
                detalhe = detalhe.PreencherValorNaLinha(206, 218,
                    infoDetalhe.ValorAbatimento.ToString().Replace(",", "").PadLeft(13, '0'));
                    // Valor do Abatimento a ser concedido
                detalhe = detalhe.PreencherValorNaLinha(219, 220,
                    infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11
                        ? "01"
                        : "02"); // Identificação do tipo de inscrição/sacado
                detalhe = detalhe.PreencherValorNaLinha(221, 234,
                    infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", ""));
                    // Nro de Inscrição do Sacado (CPF/CNPJ)
                detalhe = detalhe.PreencherValorNaLinha(235, 264, infoDetalhe.NomePagador.PadRight(30, ' '));
                    // Nome do Sacado
                detalhe = detalhe.PreencherValorNaLinha(265, 274, string.Empty.PadRight(10, '0'));
                    // Complemento de Registro
                detalhe = detalhe.PreencherValorNaLinha(275, 314, enderecoSacado.PadRight(40, ' '));
                    // Rua, Número, e Complemento do Sacado
                detalhe = detalhe.PreencherValorNaLinha(315, 326, bairroSacado.PadRight(12, ' ')); // Bairro do Sacado

                var Cep = infoDetalhe.CepPagador;

                if (Cep.Contains(".") && Cep.Contains("-"))
                    Cep = Cep.Replace(".", "").Replace("-", "");
                if (Cep.Contains("."))
                    Cep = Cep.Replace(".", "");
                if (Cep.Contains("-"))
                    Cep = Cep.Replace("-", "");

                detalhe = detalhe.PreencherValorNaLinha(327, 334, Cep.PadLeft(8, ' '));
                detalhe = detalhe.PreencherValorNaLinha(335, 349, cidadeSacado.PadRight(15, ' '));
                detalhe = detalhe.PreencherValorNaLinha(350, 351, infoDetalhe.UfPagador.PadRight(2, ' '));

                if (String.IsNullOrEmpty(infoDetalhe.NomeAvalistaOuMensagem2))
                    detalhe = detalhe.PreencherValorNaLinha(352, 381, string.Empty.PadRight(30, ' '));
                        // Nome do Sacador ou Avalista
                else
                    detalhe = detalhe.PreencherValorNaLinha(352, 381, infoDetalhe.NomeAvalistaOuMensagem2.PadRight(30, ' '));
                        // Nome do Sacador ou Avalista

                detalhe = detalhe.PreencherValorNaLinha(382, 385, string.Empty.PadRight(4, '0'));
                    // Complemento do Registro

                if (infoDetalhe.DataJurosMora == DateTime.MinValue)
                    detalhe = detalhe.PreencherValorNaLinha(386, 391, string.Empty.PadLeft(6, '0')); // Data de Mora
                else
                    detalhe = detalhe.PreencherValorNaLinha(386, 391, infoDetalhe.DataJurosMora.ToString("ddMMyy"));
                        // Data de Mora
                detalhe = detalhe.PreencherValorNaLinha(392, 393, infoDetalhe.NroDiasParaProtesto.ToString().PadLeft(2, '0'));
                    // Quantidade de Dias Posição 392 a 393
                detalhe = detalhe.PreencherValorNaLinha(394, 394, string.Empty.PadRight(1, '0'));
                    // Complemento do Registro
                detalhe = detalhe.PreencherValorNaLinha(395, 400, infoDetalhe.NumeroSequencialRegistro.ToString().PadLeft(6, '0'));
                    // Nro Sequencial do Registro no Arquivo

                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("<BoletoBr>{0}Falha na geração do DETALHE do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverTrailer(TrailerRemessaCnab400 infoTrailer)
        {
            var trailer = new string(' ', 400);
            try
            {
                trailer = trailer.PreencherValorNaLinha(1, 1, "9");
                trailer = trailer.PreencherValorNaLinha(2, 394, string.Empty.PadRight(393, ' '));
                // Contagem total de linhas do arquivo no formato '000000' - 6 dígitos
                trailer = trailer.PreencherValorNaLinha(395, 400, infoTrailer.NumeroSequencialRegistro.ToString().PadLeft(6, '0'));

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("<BoletoBr>{0}Falha na geração do TRAILER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public List<string> EscreverTexto(RemessaCnab400 remessaEscrever)
        {
            List<string> listaRetornar = new List<string>();

            listaRetornar.Add(EscreverHeader(remessaEscrever.Header));

            foreach (var detalheAdicionar in remessaEscrever.RegistrosDetalhe)
            {
                listaRetornar.AddRange(new[] { EscreverDetalhe(detalheAdicionar) });
            }

            listaRetornar.Add(EscreverTrailer(remessaEscrever.Trailer));

            return listaRetornar;
        }

        public void ValidarRemessa(RemessaCnab400 remessaValidar)
        {
            throw new NotImplementedException();
        }
    }
}
