export interface Book {
  id: number;
  name: string;
  author: string;
  genre: string;
  importDate: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}