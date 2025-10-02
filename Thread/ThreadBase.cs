using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpForm.Thread
{
    internal class ThreadBase
    {
        // 자동화 설비 상태 정의
        public enum AutomationState
        {
            Idle,           // 대기 상태
            Initialize,     // 초기화
            DataUpdate,     // 레시피 업데이트
            StartProcess,   // 작업 시작
            Processing,     // 처리 중
            QualityCheck,   // 품질 검사
            DataReport,    // 데이터 보고
            Complete,       // 작업 완료
            Error,          // 오류 상태
            Emergency       // 비상 정지
        }

        private AutomationState _currentState;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _automationTask;

        // 이벤트 정의
        public event Action<AutomationState> StateChanged;
        public event Action<string> LogMessage;
        public event Action<Exception> ErrorOccurred;

        public AutomationState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    StateChanged?.Invoke(_currentState);
                    LogMessage?.Invoke($"상태 변경: {_currentState}");
                }
            }
        }

        public bool IsRunning => _isRunning;

        public ThreadBase()
        {
            _currentState = AutomationState.Idle;
            _isRunning = false;
        }

        // 자동화 작업 시작
        public void StartAutomation()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _automationTask = Task.Run(() => AutomationLoop(_cancellationTokenSource.Token));
        }

        // 자동화 작업 정지
        public async Task StopAutomationAsync()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            if (_automationTask != null)
            {
                await _automationTask;
            }

            CurrentState = AutomationState.Idle;
        }

        // 비상 정지
        public void EmergencyStop()
        {
            CurrentState = AutomationState.Emergency;
            _cancellationTokenSource?.Cancel();
        }

        // 메인 자동화 루프
        private async Task AutomationLoop(CancellationToken cancellationToken)
        {
            try
            {
                CurrentState = AutomationState.Initialize;

                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    switch (CurrentState)
                    {
                        case AutomationState.Initialize:
                            await HandleInitialize(cancellationToken);
                            break;

                        case AutomationState.DataUpdate:
                            await HandleDataUpdate(cancellationToken);
                            break;

                        case AutomationState.StartProcess:
                            await HandleStartProcess(cancellationToken);
                            break;

                        case AutomationState.Processing:
                            await HandleProcessing(cancellationToken);
                            break;

                        case AutomationState.QualityCheck:
                            await HandleQualityCheck(cancellationToken);
                            break;

                        case AutomationState.DataReport:
                            await HandleDataReport(cancellationToken);
                            break;

                        case AutomationState.Complete:
                            await HandleComplete(cancellationToken);
                            break;

                        case AutomationState.Error:
                            await HandleError(cancellationToken);
                            break;

                        case AutomationState.Emergency:
                            await HandleEmergency(cancellationToken);
                            return; // 비상 정지 시 루프 종료

                        case AutomationState.Idle:
                            await Task.Delay(100, cancellationToken); // 짧은 대기
                            break;

                        default:
                            LogMessage?.Invoke($"알 수 없는 상태: {CurrentState}");
                            CurrentState = AutomationState.Error;
                            break;
                    }

                    // 각 사이클마다 짧은 지연
                    await Task.Delay(50, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                LogMessage?.Invoke("자동화 작업이 취소되었습니다.");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
                CurrentState = AutomationState.Error;
            }
            finally
            {
                _isRunning = false;
            }
        }

        // 각 상태별 처리 메서드들 (가상 메서드로 정의하여 상속 클래스에서 오버라이드 가능)
        protected virtual async Task HandleInitialize(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("설비 초기화 중...");

            // 초기화 작업 시뮬레이션
            await Task.Delay(1000, cancellationToken);

            // 초기화 성공 시 다음 상태로 전환
            if (CheckInitializationComplete())
            {
                CurrentState = AutomationState.StartProcess;
            }
            else
            {
                CurrentState = AutomationState.Error;
            }
        }

        protected virtual async Task HandleDataUpdate(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("데이터 업데이트 중...");

            // 데이터 업데이트 작업
            await Task.Delay(800, cancellationToken);

            // 업데이트 성공 시 다음 상태로 전환
            if (CheckDataUpdateComplete())
            {
                CurrentState = AutomationState.StartProcess;
            }
            else
            {
                CurrentState = AutomationState.Error;
            }
        }

        protected virtual async Task HandleStartProcess(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("작업 시작...");

            // 작업 시작 로직
            await Task.Delay(500, cancellationToken);

            CurrentState = AutomationState.Processing;
        }

        protected virtual async Task HandleProcessing(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("처리 중...");

            // 실제 처리 작업 (예: 기계 제어, 데이터 수집 등)
            await Task.Delay(2000, cancellationToken);

            // 처리 완료 후 품질 검사로 전환
            CurrentState = AutomationState.QualityCheck;
        }

        protected virtual async Task HandleQualityCheck(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("품질 검사 중...");

            // 품질 검사 로직
            await Task.Delay(1000, cancellationToken);

            // 검사 결과에 따라 상태 전환
            if (CheckQualityPass())
            {
                CurrentState = AutomationState.DataReport;
            }
            else
            {
                CurrentState = AutomationState.Error;
            }
        }

        protected virtual async Task HandleDataReport(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("데이터 보고 중...");

            // 데이터 보고 작업
            await Task.Delay(1200, cancellationToken);

            // 보고 성공 시 완료 상태로 전환
            if (CheckDataReportComplete())
            {
                CurrentState = AutomationState.Complete;
            }
            else
            {
                CurrentState = AutomationState.Error;
            }
        }

        protected virtual async Task HandleComplete(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("작업 완료");

            // 완료 후 처리 작업
            await Task.Delay(500, cancellationToken);

            // 연속 작업이 필요한 경우 다시 시작, 아니면 대기
            if (ShouldContinueProcess())
            {
                CurrentState = AutomationState.StartProcess;
            }
            else
            {
                CurrentState = AutomationState.Idle;
            }
        }

        protected virtual async Task HandleError(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("오류 상태 - 복구 시도 중...");

            // 오류 복구 로직
            await Task.Delay(2000, cancellationToken);

            // 복구 성공 시 초기화로, 실패 시 대기 상태로
            if (AttemptErrorRecovery())
            {
                CurrentState = AutomationState.Initialize;
            }
            else
            {
                CurrentState = AutomationState.Idle;
                _isRunning = false; // 복구 실패 시 정지
            }
        }

        protected virtual async Task HandleEmergency(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("비상 정지 상태");

            // 비상 정지 처리
            await Task.Delay(100, cancellationToken);

            _isRunning = false;
        }

        // 헬퍼 메서드들 (상속 클래스에서 구체적으로 구현)
        protected virtual bool CheckInitializationComplete() => true;
        protected virtual bool CheckDataUpdateComplete() => true;
        protected virtual bool CheckQualityPass() => true;
        protected virtual bool CheckDataReportComplete() => true;
        protected virtual bool ShouldContinueProcess() => true;
        protected virtual bool AttemptErrorRecovery() => false;

        // 외부에서 상태 강제 변경 (필요한 경우)
        public void ForceStateChange(AutomationState newState)
        {
            CurrentState = newState;
        }

        // 리소스 정리
        public void Dispose()
        {
            StopAutomationAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }
    }
}