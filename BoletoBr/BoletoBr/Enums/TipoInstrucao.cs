﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoletoBr.Enums
{
    public enum EnumTipoInstrucao
    {
        Protestar,
        NaoProtestar,
        NaoReceberAposOVencimento,
        ProtestarAposNDiasCorridos,
        ProtestarAposNDiasUteis,
        NaoReceberAposNDias,
        DevolverAposNDias,
        MultaVencimento,
        JurosdeMora,
        DescontoPorDia,
        CobrarJurosApos7DiasVencimento,

        #region Santander

        NaoHaInstrucoes,
        BaixarAposQuinzeDiasDoVencto,
        BaixarAposTrintaDiasDoVencto,
        NaoBaixar,
        NaoCobrarJurosDeMora,

        #endregion

        #region HSBC

        MultaPercentualVencimento,
        MultaPorDiaVencimento,
        MultaPorDiaCorrido,

        #endregion
    }
}
