using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Facility.Definition;
using Facility.Definition.Fsd;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Server;

namespace Facility.LanguageServer
{
	sealed class FsdSyncHandler : FsdRequestHandler, ITextDocumentSyncHandler
	{
		public FsdSyncHandler(ILanguageServer router, IDictionary<Uri, ServiceInfo> serviceInfos)
			: base(router, serviceInfos)
		{
			m_parser = new FsdParser();
		}

		public TextDocumentSyncOptions Options { get; } = new TextDocumentSyncOptions
		{
			Change = TextDocumentSyncKind.Full,
			OpenClose = true
		};

		public void SetCapability(SynchronizationCapability capability)
		{
		}

		public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
		{
			return new TextDocumentAttributes(uri, "fsd");
		}

		public async Task Handle(DidOpenTextDocumentParams notification)
		{
			await ParseAsync(notification.TextDocument.Uri, notification.TextDocument.Text).ConfigureAwait(false);
		}

		public async Task Handle(DidChangeTextDocumentParams notification)
		{
			foreach (var change in notification.ContentChanges)
			{
				await ParseAsync(notification.TextDocument.Uri, change.Text).ConfigureAwait(false);
			}
		}

		public Task Handle(DidCloseTextDocumentParams notification)
		{
			SetService(notification.TextDocument.Uri, null);
			return Task.CompletedTask;
		}

		public Task Handle(DidSaveTextDocumentParams notification)
		{
			return Task.CompletedTask;
		}

		TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
		{
			return new TextDocumentRegistrationOptions
			{
				DocumentSelector = DocumentSelector
			};
		}

		TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
		{
			return new TextDocumentChangeRegistrationOptions
			{
				DocumentSelector = DocumentSelector,
				SyncKind = Options.Change
			};
		}

		TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
		{
			return new TextDocumentSaveRegistrationOptions
			{
				DocumentSelector = DocumentSelector
			};
		}

		async Task ParseAsync(Uri documentUri, string text)
		{
			var diagnostics = new List<Diagnostic>();
			ServiceInfo service;
			IReadOnlyList<ServiceDefinitionError> errors;
			if (!m_parser.TryParseDefinition(new NamedText(documentUri.AbsoluteUri, text), out service, out errors))
			{
				diagnostics.AddRange(errors.Select(x => new Diagnostic
				{
					Severity = DiagnosticSeverity.Error,
					Message = x.Message,
					Range = new Range(new Position(x.Position), new Position(x.Position))
				}));
			}
			SetService(documentUri, service);
			Router.PublishDiagnostics(new PublishDiagnosticsParams
			{
				Uri = documentUri,
				Diagnostics = diagnostics
			});
		}

		readonly FsdParser m_parser;
	}
}
