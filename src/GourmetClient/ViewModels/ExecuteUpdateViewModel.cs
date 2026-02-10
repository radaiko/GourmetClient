using System.Threading;
using System.Threading.Tasks;
using GourmetClient.Update;
using GourmetClient.Utils;

namespace GourmetClient.ViewModels;

public class ExecuteUpdateViewModel : ViewModelBase
{
    private readonly UpdateService _updateService;

    private UpdateStepState _removePreviousVersionStepState;
    private UpdateStepState _copyNewFilesStepState;
    private UpdateStepState _cleanupStepState;
    private Task? _updateTask;

    public ExecuteUpdateViewModel()
    {
        _updateService = InstanceProvider.UpdateService;
    }

    public UpdateStepState RemovePreviousVersionStepState
    {
        get => _removePreviousVersionStepState;
        private set
        {
            _removePreviousVersionStepState = value;
            OnPropertyChanged();
        }
    }

    public UpdateStepState CopyNewFilesStepState
    {
        get => _copyNewFilesStepState;
        private set
        {
            _copyNewFilesStepState = value;
            OnPropertyChanged();
        }
    }

    public UpdateStepState CleanupStepState
    {
        get => _cleanupStepState;
        private set
        {
            _cleanupStepState = value;
            OnPropertyChanged();
        }
    }

    public override void Initialize()
    {
    }

    public async Task ExecuteUpdate(string targetPath)
    {
        Task? runningTask = _updateTask;
        if (runningTask is not null)
        {
            await runningTask;
            return;
        }

        try
        {
            _updateTask = ExecuteUpdate(targetPath, CancellationToken.None);
            await _updateTask;
        }
        finally
        {
            _updateTask = null;
        }
    }

    private async Task ExecuteUpdate(string targetPath, CancellationToken cancellationToken)
    {
        RemovePreviousVersionStepState = UpdateStepState.Running;

        try
        {
            await _updateService.RemovePreviousVersion(targetPath, cancellationToken);
        }
        catch (GourmetUpdateException)
        {
            RemovePreviousVersionStepState = UpdateStepState.Error;
            throw;
        }

        RemovePreviousVersionStepState = UpdateStepState.Finished;
        CopyNewFilesStepState = UpdateStepState.Running;

        try
        {
            await _updateService.CopyCurrentVersion(targetPath, cancellationToken);
        }
        catch (GourmetUpdateException)
        {
            CopyNewFilesStepState = UpdateStepState.Error;
            throw;
        }

        CopyNewFilesStepState = UpdateStepState.Finished;
        CleanupStepState = UpdateStepState.Running;

        try
        {
            await _updateService.RemoveUpdateFiles(CancellationToken.None);
        }
        catch (GourmetUpdateException)
        {
            CleanupStepState = UpdateStepState.Error;
            throw;
        }

        CleanupStepState = UpdateStepState.Finished;

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _updateService.StartNewVersion(targetPath);
    }
}