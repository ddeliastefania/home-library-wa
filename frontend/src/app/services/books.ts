
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult, Book } from '../models/book';

export interface ImportResponse {
    acceptedRows: number;
}

@Injectable({
    providedIn: 'root'
})

export class BooksService {
    private readonly http = inject(HttpClient);

    private readonly apiUrl = 'http://localhost:8080/api';

    getBooks(): Observable<Book[]> {
        return this.http.get<Book[]>(`${this.apiUrl}/books`);
    }

    getPagedBooks(page: number, pageSize: number) {
        return this.http.get<PagedResult<Book>>(
            `${this.apiUrl}/books/paged`,
            {
                params: {
                    page: page.toString(),
                    pageSize: pageSize.toString()
                }
            }
        );
    }

    importBooks(file: File): Observable<ImportResponse> {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<ImportResponse>(
            `${this.apiUrl}/imports`,
            formData
        );
    }
}
