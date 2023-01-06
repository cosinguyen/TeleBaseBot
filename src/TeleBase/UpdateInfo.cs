using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TeleBaseBotFW.TeleBase
{
    public enum UpdateKind { None, NewMessage, EditedMessage, CallbackQuery, OtherUpdate }
    public enum MsgCategory { Other, Text, MediaOrDoc, StickerOrDice, Sharing, ChatStatus, VideoChat }
    public enum ReplyKind { Message, CallbackData, All }

    internal class UpdateInfo : IGetNext
    {
        internal UpdateKind UpdateKind;
        internal Message Message;
        internal string? CallbackData;
        internal Update Update;
        internal TaskInfo? TaskInfo;

        public MsgCategory MsgCategory => (Message?.Type) switch
        {
            MessageType.Text => MsgCategory.Text,
            MessageType.Photo or MessageType.Audio or MessageType.Video or MessageType.Voice or MessageType.Document or MessageType.VideoNote
              => MsgCategory.MediaOrDoc,
            MessageType.Sticker or MessageType.Dice
              => MsgCategory.StickerOrDice,
            MessageType.Location or MessageType.Contact or MessageType.Venue or MessageType.Game or MessageType.Invoice or
            MessageType.SuccessfulPayment or MessageType.WebsiteConnected
              => MsgCategory.Sharing,
            MessageType.ChatMembersAdded or MessageType.ChatMemberLeft or MessageType.ChatTitleChanged or MessageType.ChatPhotoChanged or
            MessageType.MessagePinned or MessageType.ChatPhotoDeleted or MessageType.GroupCreated or MessageType.SupergroupCreated or
            MessageType.ChannelCreated or MessageType.MigratedToSupergroup or MessageType.MigratedFromGroup
              => MsgCategory.ChatStatus,
            MessageType.VideoChatScheduled or MessageType.VideoChatStarted or MessageType.VideoChatEnded or MessageType.VideoChatParticipantsInvited
              => MsgCategory.VideoChat,
            _ => MsgCategory.Other,
        };

        internal UpdateInfo(UpdateKind updateKind, Update update, Message message)
        {
            UpdateKind = updateKind;
            Update = update;
            Message = message;
        }

        async Task<UpdateInfo> IGetNext.NextUpdateAsync(CancellationToken cancel)
        {
            UpdateInfo newUpdate = this;

            if (TaskInfo != null)
            {
                await TaskInfo.Semaphore.WaitAsync(cancel);

                lock (TaskInfo)
                    newUpdate = TaskInfo.Updates.Dequeue();
            }

            return newUpdate;
        }
    }
}