using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ResoConsultant.Services
{
    public class RagService
    {
        private readonly List<DocumentChunk> _documentChunks;
        private readonly string _documentsPath;

        public RagService()
        {
            // Автоматически находим папку documents рядом с приложением
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _documentsPath = Path.Combine(baseDir, "documents");
            _documentChunks = new List<DocumentChunk>();

            InitializeKnowledgeBase();
            LoadDocuments();
        }

        private void InitializeKnowledgeBase()
        {
            if (!Directory.Exists(_documentsPath))
            {
                Directory.CreateDirectory(_documentsPath);
            }
        }

        private void LoadDocuments()
        {
            try
            {
                _documentChunks.Clear();
                if (!Directory.Exists(_documentsPath)) return;

                var files = Directory.GetFiles(_documentsPath, "*.txt");
                Console.WriteLine($"[RAG] Найдено файлов для загрузки: {files.Length}");

                int id = 1;
                foreach (var f in files)
                {
                    // Читаем строки, чтобы разбить файл на логические абзацы
                    var lines = File.ReadAllLines(f);
                    string currentChunk = "";

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        currentChunk += line + " ";

                        // Если абзац стал больше 600 символов, сохраняем его как отдельный кусок
                        if (currentChunk.Length > 600)
                        {
                            _documentChunks.Add(new DocumentChunk
                            {
                                Id = id++,
                                Content = currentChunk.Trim(),
                                Source = Path.GetFileName(f)
                            });
                            currentChunk = "";
                        }
                    }

                    // Добавляем оставшийся текст файла
                    if (!string.IsNullOrEmpty(currentChunk))
                    {
                        _documentChunks.Add(new DocumentChunk
                        {
                            Id = id++,
                            Content = currentChunk.Trim(),
                            Source = Path.GetFileName(f)
                        });
                    }
                }
                Console.WriteLine($"[RAG] База знаний успешно загружена. Всего фрагментов: {_documentChunks.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RAG] Ошибка при загрузке документов: {ex.Message}");
            }
        }

        public string GetSystemPromptWithContext(string query)
        {
            if (!_documentChunks.Any())
            {
                return "Ты — полезный ассистент страховой компании РЕСО-МЕД. Отвечай вежливо.";
            }

            // Ищем наиболее подходящие куски текста по ключевым словам из вопроса
            var keywords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var relevant = _documentChunks
                .Select(chunk => new {
                    Chunk = chunk,
                    Score = keywords.Count(k => chunk.Content.ToLower().Contains(k))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(4) // Берем до 4 самых релевантных фрагментов
                .Select(x => x.Chunk.Content);

            string context = relevant.Any()
                ? string.Join("\n---\n", relevant)
                : "Информации в базе не найдено, ответь на основе общих знаний о страховании.";

            // Формируем четкую инструкцию для нейронки
            return $@"Ты — официальный чат-бот страховой компании РЕСО-МЕД. 
Используй предоставленный КОНТЕКСТ ниже, чтобы ответить на вопрос пользователя. 
Если в контексте нет ответа, так и скажи, не придумывай лишнего.

КОНТЕКСТ:
{context}

Инструкции: Отвечай кратко, профессионально и только по делу.";
        }
    }

    public class DocumentChunk
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public string Source { get; set; } = "";
    }
}
