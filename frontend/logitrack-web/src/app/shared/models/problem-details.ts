// Errores de la API en formato ProblemDetails (RFC 9457). "errors" trae mensajes por campo en las validaciones.
export interface ProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly traceId?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}
