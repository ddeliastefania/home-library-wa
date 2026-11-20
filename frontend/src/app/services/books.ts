
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../models/book';

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

    importBooks(file: File): Observable<ImportResponse> {
        const formData = new FormData();
        formData.append('file', file);

        return this.http.post<ImportResponse>(
            `${this.apiUrl}/imports`,
            formData
        );
    }
}
