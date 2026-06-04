using FlightSystem.Domain.Entities;
using FlightSystem.Domain.Interfaces;

namespace FlightSystem.UseCases
{
    public class CrashPlaneUseCase
    {
        private readonly IPlaneCrashPresenter _presenter;

        public CrashPlaneUseCase(IPlaneCrashPresenter presenter)
        {
            _presenter = presenter;
        }

        public void Execute(PlaneState state)
        {
            if (state.isCrashed) return;

            state.Crash();
            _presenter.PresentCrash();
        }
    }
}
