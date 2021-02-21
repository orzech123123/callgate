using AutoMapper;
using CallGate.ApiModels.Channel;
using CallGate.ApiModels.ChannelMessage;
using CallGate.ApiModels.Chat;
using CallGate.ApiModels.ChatMessage;
using CallGate.ApiModels.Group;
using CallGate.ApiModels.User;
using CallGate.Documents;
using ChatUser = CallGate.Models.ChatUser;

namespace CallGate.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ChatUser, UserResponse>()
                .ForMember(userResponse => userResponse.Id, option => option.MapFrom(chatUser => chatUser.UserId))
                .ForMember(userResponse => userResponse.Username, option => option.MapFrom(chatUser => chatUser.User.Username));

            CreateMap<Models.Chat, ChatResponse>()
                .ForMember(chatResponse => chatResponse.Users, option => option.MapFrom(chat => chat.ChatUsers));
            
            CreateMap<Models.GroupUser, UserResponse>()
                .ForMember(userResponse => userResponse.Id, option => option.MapFrom(groupUser => groupUser.UserId))
                .ForMember(userResponse => userResponse.Username, option => option.MapFrom(groupUser => groupUser.User.Username));
                
            CreateMap<Models.GroupUser, UserDetailsResponse>()
                .ForMember(ud => ud.Id, option => option.MapFrom(groupUser => groupUser.UserId))
                .ForMember(ud => ud.Username, option => option.MapFrom(groupUser => groupUser.User.Username))
                .ForMember(ud => ud.Role, option => option.MapFrom(groupUser => groupUser.Role))
                .ForMember(ud => ud.JoinedGroupAt, option => option.MapFrom(groupUser => groupUser.JoinedAt));

            CreateMap<ChatMessage, ChatMessageResponse>()
                .ForMember(cmr => cmr.UserId, option => option.MapFrom(cm => cm.UserId))
                .ForMember(cmr => cmr.Username, option => option.MapFrom(cm => cm.Username));
            
            CreateMap<ChannelMessage, ChannelMessageResponse>()
                .ForMember(cmr => cmr.UserId, option => option.MapFrom(cm => cm.UserId))
                .ForMember(cmr => cmr.Username, option => option.MapFrom(cm => cm.Username));

            CreateMap<CallGate.Models.User, UserFullResponse>();
            CreateMap<CallGate.Models.User, UserResponse>();
            CreateMap<CallGate.Models.Group, GroupResponse>();
            CreateMap<CallGate.Models.Channel, ChannelResponse>();
            CreateMap<CallGate.Documents.Message, ChatMessage>();
        }
    }
}