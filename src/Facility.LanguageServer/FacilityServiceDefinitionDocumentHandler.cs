using System;
using System.Collections.Generic;
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
	public sealed class FacilityServiceDefinitionDocumentHandler : ITextDocumentSyncHandler
	{
		public FacilityServiceDefinitionDocumentHandler(ILanguageServer router)
		{
			m_router = router;

			m_parser = new FsdParser();
		}

		public TextDocumentSyncOptions Options { get; } = new TextDocumentSyncOptions
		{
			Change = TextDocumentSyncKind.Full,
			OpenClose = true
		};

		public void SetCapability(SynchronizationCapability capability)
		{
			m_capability = capability;
		}

		public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
		{
			return new TextDocumentAttributes(uri, "fsd");
		}

		public async Task Handle(DidOpenTextDocumentParams notification)
		{
			await Parse(notification.TextDocument.Uri, notification.TextDocument.Text).ConfigureAwait(false);
		}

		public async Task Handle(DidChangeTextDocumentParams notification)
		{
			foreach (var change in notification.ContentChanges)
			{
				await Parse(notification.TextDocument.Uri, change.Text).ConfigureAwait(false);
			}
		}

		public Task Handle(DidCloseTextDocumentParams notification)
		{
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
				DocumentSelector = m_documentSelector
			};
		}

		TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
		{
			return new TextDocumentChangeRegistrationOptions
			{
				DocumentSelector = m_documentSelector,
				SyncKind = Options.Change
			};
		}

		TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
		{
			return new TextDocumentSaveRegistrationOptions
			{
				DocumentSelector = m_documentSelector
			};
		}

		private async Task Parse(Uri documentUri, string text)
		{
			var diagnostics = new List<Diagnostic>();
			try
			{
				m_parser.ParseDefinition(new NamedText(documentUri.AbsoluteUri, text));
			}
			catch (ServiceDefinitionException ex)
			{
				diagnostics.Add(new Diagnostic
				{
					Severity = DiagnosticSeverity.Error,
					Message = ex.Error,
					Range = new Range(ToLspPosition(ex.Position), ToLspPosition(ex.Position))
				});
			}
			m_router.PublishDiagnostics(new PublishDiagnosticsParams
			{
				Uri = documentUri,
				Diagnostics = diagnostics
			});
		}

		private static Position ToLspPosition(NamedTextPosition position)
		{
			return new Position(position.LineNumber - 1, position.ColumnNumber - 1);
		}

		private readonly ILanguageServer m_router;
		private readonly FsdParser m_parser;

		private readonly DocumentSelector m_documentSelector = new DocumentSelector(
			new DocumentFilter()
			{
				Pattern = "**/*.fsd",
				Language = "fsd"
			}
		);

		private SynchronizationCapability m_capability;
	}
}