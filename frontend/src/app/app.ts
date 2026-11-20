
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BooksService } from './services/books';
import { Book } from './models/book';
import { timer, switchMap, take } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit {
  private readonly booksService = inject(BooksService);

  books = signal<Book[]>([]);
  loading = signal(true);
  errorMessage = signal('');

  uploading = signal(false);
  uploadMessage = signal('');
  isDragging = signal(false);


  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      this.uploadFile(file);
    }

    input.value = '';
  }

  uploadFile(file: File): void {
    if (this.uploading()) {
      return;
    }

    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.uploadMessage.set('Please select a CSV file.');
      return;
    }

    this.uploading.set(true);
    this.uploadMessage.set('');

    this.booksService.importBooks(file).subscribe({
      next: (response) => {
        this.uploadMessage.set(
          `${response.acceptedRows} books accepted for import.`
        );

        this.uploading.set(false);
        this.refreshBooksAfterImport();
      },
      error: (error) => {
        console.error('Failed to import books:', error);
        this.uploadMessage.set('Failed to import CSV file.');
        this.uploading.set(false);
      }
    });
  }


  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);

    if (this.uploading()) {
      return;
    }

    const file = event.dataTransfer?.files[0];

    if (file) {
      this.uploadFile(file);
    }
  }

  ngOnInit(): void {
    this.loadBooks();
  }
  // COMM
  // loadbooks este apelat dupa import pentru ca API-ul publica mesajele in RabbitMQ
  // iar workerul le salveaza asincron 
  // asta se face pentru ca lista sa nu fie reincarcata inainte ca workerul sa termine procesarea 
  // pentru moment avem butonul referresh, ulterior o sa facem actualizare automata. 

  loadBooks(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.booksService.getBooks().subscribe({
      next: (books) => {
        console.log('Books received from API:', books);

        this.books.set(books);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load books:', error);

        this.errorMessage.set('Nu am putut încărca lista cărților.');
        this.loading.set(false);
      }
    });
  }

  // COMM 
  // asteapta 500ms si reincarca lista la fiecare secunda 
  // face maxim 5 cereri
  // actualizeaza tabelul automat pe masura de workerul salveaza cartile 
  refreshBooksAfterImport(): void {
    timer(500, 1000)
      .pipe(
        take(5),
        switchMap(() => this.booksService.getBooks())
      )
      .subscribe({
        next: (books) => {
          this.books.set(books);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Failed to refresh books:', error);
        }
      });
  }
}