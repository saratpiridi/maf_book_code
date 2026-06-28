# Chapter 6: Retrieval (RAG)

The retrieval sample turns the agent into a workplace-helpdesk assistant grounded
in five in-memory policy documents. The questions cross HR leave, expenses, IT,
facilities, and security so you can see retrieval route to different domains and
append citations.

You also need an embedding deployment in the repo-root `.env`:

    AZURE_OPENAI_EMBEDDING_DEPLOYMENT=text-embedding-3-small

Listing guide:

- Listing 6-3: `HelpdeskDocument`, the vector-searchable document model.
- Listing 6-4: the in-memory helpdesk documents.
- Listing 6-5: building the chat client, embedding generator, and in-memory store.
- Listing 6-6: the `TextSearchProvider` retrieval context provider.
- Listing 6-7: the search function over the vector store collection.
- Listing 6-8: attaching retrieval to the agent via `AIContextProviders`.
- Listing 6-9: the five helpdesk questions.
- Listing 6-10: the embedding generator factory.
- Listing 6-11: captured output for the five questions.

Run from the repository root:

    dotnet run --project ./src/Ch06.Rag/Ch06.Rag.csproj
