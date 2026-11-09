using Microsoft.ML.Tokenizers;

namespace SmartSupport.Api.AI.Tokenizer
{
    public class TokenizerService
    {
        private readonly BertTokenizer _tokenizer;

        public TokenizerService()
        {
            var vocabPath = Path.Combine(
                AppContext.BaseDirectory,
                "AI",
                "Tokenizers",
                "vocab.txt");

            if (!File.Exists(vocabPath))
            {
                throw new FileNotFoundException(
                    "Tokenizer vocabulary file was not found.",
                    vocabPath);
            }

            var options = new BertOptions
            {
                LowerCaseBeforeTokenization = true,
                ApplyBasicTokenization = true
            };

            _tokenizer = BertTokenizer.Create(vocabPath, options);
        }

        public int CountTokens(string text)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            return _tokenizer
                .EncodeToIds(text, addSpecialTokens: false)
                .Count;
        }
    }
}
