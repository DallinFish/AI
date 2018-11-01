﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace PointOfInterestSkill
{
    public class CancelDialog : ComponentDialog
    {
        // Constants
        public const string CancelPrompt = "cancelPrompt";

        // Fields
        private IStatePropertyAccessor<PointOfInterestSkillState> _accessor;
        private static CancelResponses _responder = new CancelResponses();

        public CancelDialog(IStatePropertyAccessor<PointOfInterestSkillState> accessor)
            : base(nameof(CancelDialog))
        {
            _accessor = accessor;

            InitialDialogId = nameof(CancelDialog);

            var cancel = new WaterfallStep[]
            {
                AskToCancel,
                FinishCancelDialog,
            };

            AddDialog(new WaterfallDialog(InitialDialogId, cancel));
            AddDialog(new ConfirmPrompt(CancelPrompt, null, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) { Style = ListStyle.SuggestedAction });
        }

        public static async Task<DialogTurnResult> AskToCancel(WaterfallStepContext sc, CancellationToken cancellationToken) => await sc.PromptAsync(CancelPrompt, new PromptOptions()
        {
            Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, CancelResponses._confirmPrompt),
        });

        public static async Task<DialogTurnResult> FinishCancelDialog(WaterfallStepContext sc, CancellationToken cancellationToken) => await sc.EndDialogAsync((bool)sc.Result);

        protected override async Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            var doCancel = (bool)result;

            if (doCancel)
            {
                // clear state
                var state = await _accessor.GetAsync(outerDc.Context, () => new PointOfInterestSkillState());
                state.Clear();

                // If user chose to cancel
                await _responder.ReplyWith(outerDc.Context, CancelResponses._cancelConfirmed);

                // Cancel all in outer stack of component i.e. the stack the component belongs to
                return await outerDc.CancelAllDialogsAsync();
            }
            else
            {
                // else if user chose not to cancel
                await _responder.ReplyWith(outerDc.Context, CancelResponses._cancelDenied);

                // End this component. Will trigger reprompt/resume on outer stack
                return await outerDc.EndDialogAsync();
            }
        }
    }
}