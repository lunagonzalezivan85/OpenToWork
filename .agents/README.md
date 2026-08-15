# OpenToWork - Agentes

Este directorio define los 5 agentes del proyecto y el flujo de trabajo. Cada agente tiene un rol especifico.

## Agentes

| Agente | Archivo | Rol |
|---|---|---|
| **PM** | `pm.md` | Project Manager - controla el flujo del proyecto, coordina agentes, gestiona fases y riesgos |
| **QA** | `qa.md` | Quality Assurance - valida diseno, funcionalidad, calidad de informacion y i18n |
| **FS** | `fs.md` | Full Stack Developer - experto en Blazor, C#, CSS, JS, HTML. Frontend y backend |
| **SEC** | `sec.md` | Security Specialist - audita vulnerabilidades, JWT, encriptacion, inputs, dependencias |
| **RH** | `rh.md` | Reclutamiento y Seleccion - mapeo de perfiles, sourcing, criba, evaluacion, entrevista, oferta y onboarding |

## Flujo de Trabajo

| Archivo | Descripcion |
|---|---|
| `WORKFLOW.md` | Flujo de trabajo por fase - 7 etapas secuenciales, no se puede saltar fases |

### Flujo por fase (7 etapas)

```
[1] Planificacion (PM)
      v
[2] Diseno Tecnico (PM + FS + SEC review)
      v
[3] Implementacion (FS) --build OK--> [4] Pruebas (QA)
                                          |
                                     bugs? v no bugs
                                     [6] Correcciones   [5] Auditoria (SEC)
                                          |                  |
                                     <--- re-valida <---+    |
                                                               v
                                                         [7] Cierre (PM)
                                                               |
                                                         FASE COMPLETADA
```

### Regla principal

**Ninguna fase puede iniciar hasta que la fase anterior este 100% completada y firmada por PM, QA y SEC.**

Ver `WORKFLOW.md` para el detalle completo de cada etapa, gates de aprobacion y reglas estrictas.
