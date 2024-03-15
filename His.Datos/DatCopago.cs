using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using His.Entidades;

namespace His.Datos
{
    public class DatCopago
    {
        public COPAGO recuperaCopago(Int64 ate_codigo)
        {
            using (var db = new HIS3000BDEntities(ConexionEntidades.ConexionEDM))
            {
                return db.COPAGO.FirstOrDefault(x => x.ATE_CODIGO_COPAGO == ate_codigo);
            }
        }

        public List<DtoReporteCopago> valoresCuentas(Int64 ate_codigo)
        {
            using (var db = new HIS3000BDEntities(ConexionEntidades.ConexionEDM))
            {
                List<DtoReporteCopago> reporte = new List<DtoReporteCopago>();
                var result = (from cuenta in db.CUENTAS_PACIENTES
                              join rubro in db.RUBROS on cuenta.RUB_CODIGO equals rubro.RUB_CODIGO
                              where cuenta.ATE_CODIGO == ate_codigo && cuenta.CUE_ESTADO == 1
                              group new { cuenta.CUE_CANTIDAD, cuenta.CUE_DETALLE, cuenta.CUE_IVA, cuenta.Descuento, cuenta.CUE_VALOR, rubro.RUB_NOMBRE }
                              by new { cuenta.PRO_CODIGO, cuenta.CUE_DETALLE, rubro.RUB_NOMBRE, cuenta.Descuento } into g
                              select new
                              {
                                  PRO_CODIGO = g.Key.PRO_CODIGO,
                                  CANTIDAD = Math.Round((decimal)g.Sum(x => x.CUE_CANTIDAD), 2),
                                  VALOR_UNITARIO = Math.Round((decimal)g.Sum(x => x.CUE_VALOR), 3),
                                  DETALLE = g.Key.CUE_DETALLE,
                                  IVA = Math.Round((decimal)g.Sum(x => x.CUE_IVA), 4),
                                  TOTAL = g.Sum(x => x.CUE_VALOR),
                                  g.Key.RUB_NOMBRE,
                                  DESCUENTO = Math.Round(g.Sum(x => x.Descuento), 2)
                              }).ToList();

                foreach (var item in result)
                {
                    DtoReporteCopago rcop = new DtoReporteCopago();
                    rcop.PRO_CODIGO = item.PRO_CODIGO;
                    rcop.CANTIDAD = (long)item.CANTIDAD;
                    rcop.DETALLE = item.DETALLE;
                    rcop.VALOR_UNITARIO = item.VALOR_UNITARIO;
                    rcop.IVA = item.IVA;
                    rcop.TOTAL = (decimal)item.TOTAL;
                    rcop.DESCUENTO = (decimal)item.DESCUENTO;
                    reporte.Add(rcop);
                }
                return reporte;
            }
        }
        public List<DtoReporteCopago> cuentaAuditoria(Int64 ate_codigo)
        {
            //using (var db=new HIS3000BDEntities(ConexionEntidades.ConexionEDM))
            //{
            //    return db.CUENTAS_PACIENTES_COPAGO.Where(x => x.ATE_CODIGO == ate_codigo).ToList();
            //}
            using (var db = new HIS3000BDEntities(ConexionEntidades.ConexionEDM))
            {
                List<DtoReporteCopago> reporte = new List<DtoReporteCopago>();
                var result = (from cuenta in db.CUENTAS_PACIENTES_COPAGO
                              join rubro in db.RUBROS on cuenta.RUB_CODIGO equals rubro.RUB_CODIGO
                              where cuenta.ATE_CODIGO == ate_codigo && cuenta.CUE_ESTADO == 1
                              group new { cuenta.CUE_CANTIDAD, cuenta.CUE_DETALLE, cuenta.CUE_IVA, cuenta.Descuento, cuenta.CUE_VALOR_UNITARIO, rubro.RUB_NOMBRE }
                              by new { cuenta.PRO_CODIGO, cuenta.CUE_DETALLE, rubro.RUB_NOMBRE, cuenta.Descuento } into g
                              select new
                              {
                                  PRO_CODIGO = g.Key.PRO_CODIGO,
                                  CANTIDAD = Math.Round((decimal)g.Sum(x => x.CUE_CANTIDAD), 2),
                                  VALOR_UNITARIO = Math.Round((decimal)g.Sum(x => x.CUE_VALOR_UNITARIO), 3),
                                  DETALLE = g.Key.CUE_DETALLE,
                                  IVA = Math.Round((decimal)g.Sum(x => x.CUE_IVA), 4),
                                  TOTAL = Math.Round((decimal)g.Sum(x => x.CUE_VALOR_UNITARIO * x.CUE_CANTIDAD), 2),
                                  g.Key.RUB_NOMBRE,
                                  DESCUENTO = Math.Round(g.Sum(x => x.Descuento), 2)
                              }).ToList();

                foreach (var item in result)
                {
                    DtoReporteCopago rcop = new DtoReporteCopago();
                    rcop.PRO_CODIGO = item.PRO_CODIGO;
                    rcop.CANTIDAD = (long)item.CANTIDAD;
                    rcop.DETALLE = item.DETALLE;
                    rcop.VALOR_UNITARIO = item.VALOR_UNITARIO;
                    rcop.IVA = item.IVA;
                    rcop.TOTAL = item.TOTAL + item.IVA;
                    rcop.DESCUENTO = (decimal)item.DESCUENTO;
                    reporte.Add(rcop);
                }
                return reporte;
            }
        }
        public List<DtopAnulaCopago> cuentasCopago()
        {
            using (var db = new HIS3000BDEntities(ConexionEntidades.ConexionEDM))
            {
                List<DtopAnulaCopago> vistaanula = new List<DtopAnulaCopago>();
                var resultados = (from c in db.COPAGO
                                  join a in db.ATENCIONES on c.ATE_CODIGO_COPAGO equals a.ATE_CODIGO
                                  join p in db.PACIENTES on a.PACIENTES.PAC_CODIGO equals p.PAC_CODIGO
                                  join cp in db.CUENTAS_PACIENTES on a.ATE_CODIGO equals cp.ATE_CODIGO
                                  join ac in db.ATENCION_DETALLE_CATEGORIAS on a.ATE_CODIGO equals ac.ATENCIONES.ATE_CODIGO
                                  join cc in db.CATEGORIAS_CONVENIOS on ac.CATEGORIAS_CONVENIOS.CAT_CODIGO equals cc.CAT_CODIGO
                                  group new { a, p, cp, cc } by new
                                  {
                                      a.ATE_CODIGO,
                                      a.ATE_NUMERO_ATENCION,
                                      p.PAC_HISTORIA_CLINICA,
                                      Paciente = p.PAC_APELLIDO_PATERNO + " " + p.PAC_APELLIDO_MATERNO + " " + p.PAC_NOMBRE1 + " " + p.PAC_NOMBRE2,
                                      Aseguradora = cc.CAT_NOMBRE
                                  } into g
                                  select new
                                  {
                                      g.Key.ATE_CODIGO,
                                      g.Key.ATE_NUMERO_ATENCION,
                                      g.Key.PAC_HISTORIA_CLINICA,
                                      g.Key.Paciente,
                                      T_CUENTA = g.Sum(x => x.cp.CUE_VALOR + x.cp.CUE_IVA),
                                      g.Key.Aseguradora
                                  }).ToList();
                foreach (var item in resultados)
                {
                    DtopAnulaCopago acp = new DtopAnulaCopago();
                    acp.C_ATENCION = item.ATE_CODIGO;
                    acp.ATENCION = item.ATE_NUMERO_ATENCION;
                    acp.HC = item.PAC_HISTORIA_CLINICA;
                    acp.PACIENTE = item.Paciente;
                    acp.ASEGURADORA = item.Aseguradora;
                    acp.T_CUENTA = (decimal)item.T_CUENTA;
                    vistaanula.Add(acp);
                }
                return vistaanula;
            }
        }
    }
}
