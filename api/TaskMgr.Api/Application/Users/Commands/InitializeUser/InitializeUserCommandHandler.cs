using AutoMapper;
using MediatR;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Users.Commands.InitializeUser;

public class InitializeUserCommandHandler : IRequestHandler<InitializeUserCommand, InitializeUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InitializeUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InitializeUserResult> Handle(InitializeUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetSingleAsync(
            u => u.AzureObjectId == request.AzureObjectId,
            cancellationToken);

        var isNewUser = existingUser == null;

        User user;
        if (existingUser != null)
        {
            existingUser.UpdateProfile(request.DisplayName);
            existingUser.RecordLogin();
            _unitOfWork.Users.Update(existingUser);
            user = existingUser;
        }
        else
        {
            user = new User(request.Email, request.DisplayName, request.AzureObjectId);
            user.RecordLogin();
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InitializeUserResult(_mapper.Map<Application.DTOs.UserDto>(user), isNewUser);
    }
}
