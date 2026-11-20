
import { Component, computed, inject, OnInit, signal } from '@angular/core';
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

  currentPage = signal(1);
  pageSize = 10;
  totalCount = signal(0);

  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize)
  );

  loading = signal(true);
  errorMessage = signal('');

  uploading = signal(false);
  uploadMessage = signal('');
  uploadError = signal(false);
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

    this.uploadError.set(false);
    this.uploadMessage.set('');

    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.uploadError.set(true);
      this.uploadMessage.set('Invalid file format. Please upload a CSV file.');
      return;
    }

    this.uploading.set(true);

    this.booksService.importBooks(file).subscribe({
      next: (response) => {
        this.uploadMessage.set(
          `${response.acceptedRows} books accepted for import.`
        );

        this.uploadError.set(false);
        this.uploading.set(false);

        this.refreshBooksAfterImport();
      },
      error: (error) => {
        console.error('Failed to import books:', error);

        this.uploadError.set(true);
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

  loadBooks(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.booksService
      .getPagedBooks(this.currentPage(), this.pageSize)
      .subscribe({
        next: (result) => {
          this.books.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Failed to load books:', error);
          this.errorMessage.set('Failed to load books.');
          this.loading.set(false);
        }
      });
  }

  changePage(page: number): void {
    if (
      page < 1 ||
      page > this.totalPages() ||
      page === this.currentPage() ||
      this.loading()
    ) {
      return;
    }

    this.currentPage.set(page);
    this.loadBooks();
  }

  refreshBooksAfterImport(): void {
    this.currentPage.set(1);

    timer(500, 1000)
      .pipe(
        take(5),
        switchMap(() =>
          this.booksService.getPagedBooks(
            this.currentPage(),
            this.pageSize
          )
        )
      )
      .subscribe({
        next: (result) => {
          this.books.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Failed to refresh books:', error);
        }
      });
  }
}
