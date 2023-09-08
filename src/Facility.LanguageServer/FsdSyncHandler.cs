using Facility.Definition;
using Facility.Definition.Fsd;
using Facility.Definition.Http;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer
{
	internal sealed class FsdSyncHandler : FsdRequestHandler, ITextDocumentSyncHandler
	{
		public FsdSyncHandler(
			ILanguageServerConfiguration configuration,
			IDictionary<DocumentUri, ServiceInfo> serviceInfos)
			: base(configuration, serviceInfos)
		{
			m_parser = new FsdParser();
		}

		public TextDocumentSyncOptions Options { get; } =
			new TextDocumentSyncOptions
			{
				Change = TextDocumentSyncKind.Full,
				OpenClose = true,
			};

		public void SetCapability(SynchronizationCapability capability)
		{
		}

		public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
		{
			return new TextDocumentAttributes(uri, "fsd");
		}

		public async Task<Unit> Handle(DidOpenTextDocumentParams notification, CancellationToken cancellationToken)
		{
			await ParseAsync(notification.TextDocument.Uri, notification.TextDocument.Text).ConfigureAwait(false);
			await Configuration.GetScopedConfiguration(notification.TextDocument.Uri, cancellationToken).ConfigureAwait(false);
			return Unit.Value;
		}

		public async Task<Unit> Handle(DidChangeTextDocumentParams notification, CancellationToken cancellationToken)
		{
			foreach (var change in notification.ContentChanges)
			{
				await ParseAsync(notification.TextDocument.Uri, change.Text).ConfigureAwait(false);
			}
			return Unit.Value;
		}

		public async Task<Unit> Handle(DidCloseTextDocumentParams notification, CancellationToken cancellationToken)
		{
			SetService(notification.TextDocument.Uri, null);

			return Unit.Value;
		}

		public async Task<Unit> Handle(DidSaveTextDocumentParams notification, CancellationToken cancellationToken)
		{
			return Unit.Value;
		}

		public TextDocumentOpenRegistrationOptions GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentOpenRegistrationOptions
			{
				DocumentSelector = DocumentSelector,
			};
		}

		TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentCloseRegistrationOptions()
			{
				DocumentSelector = DocumentSelector,
			};
		}

		TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentChangeRegistrationOptions
			{
				DocumentSelector = DocumentSelector,
				SyncKind = Options.Change,
			};
		}

		TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentSaveRegistrationOptions
			{
				DocumentSelector = DocumentSelector,
			};
		}

		private async Task ParseAsync(DocumentUri documentUri, string text)
		{
			var diagnostics = new List<Diagnostic>();

			if (!m_parser.TryParseDefinition(new ServiceDefinitionText(documentUri.ToUri().AbsoluteUri, text), out var service, out var errors))
				diagnostics.AddRange(errors.Select(ToDiagnostic));

			if (service != null && !HttpServiceInfo.TryCreate(service, out _, out errors))
				diagnostics.AddRange(errors.Select(ToDiagnostic));

			SetService(documentUri, service);

			// Router.PublishDiagnostics(new PublishDiagnosticsParams
			// {
			// 	Uri = documentUri.ToUri(),
			// 	Diagnostics = diagnostics,
			// });
		}

		private static Diagnostic ToDiagnostic(ServiceDefinitionError error) =>
			new Diagnostic
			{
				Severity = DiagnosticSeverity.Error,
				Message = error.Message,
				Range = new Range(new Position(error.Position), new Position(error.Position)),
			};

		private readonly FsdParser m_parser;
	}
}
