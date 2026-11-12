// Forma de las respuestas paginadas de la API (PagedResult<T> del backend).
export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}
