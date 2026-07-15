import { ref, readonly, onUnmounted } from 'vue';
import { HubConnectionBuilder, HubConnection, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { logger } from '@/services/logger';
import { config } from '@/services/config';
import { useNotification } from '@/composables/useNotification';

const connection = ref<HubConnection | null>(null);
const isConnected = ref(false);
const isConnecting = ref(false);
const reconnectAttempts = ref(0);

export function useSignalR() {
    const { showNotification } = useNotification();
    const signalRConfig = config.get('signalR');

    const startConnection = async (tokenProvider?: () => Promise<string | null>) => {
        if (connection.value || isConnecting.value) {
            logger.warn('SignalR connection already exists or is connecting');
            return;
        }

        isConnecting.value = true;

        try {
            logger.info('Starting SignalR connection...');

            connection.value = new HubConnectionBuilder()
                .withUrl(signalRConfig.hubUrl, {
                    accessTokenFactory: async () => {
                        if (!tokenProvider) return '';
                        const token = await tokenProvider();
                        return token || '';
                    },
                    transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling
                })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: (retryContext) => {
                        const delay = Math.min(
                            signalRConfig.retryDelay * Math.pow(2, retryContext.previousRetryCount),
                            30000 // Max 30 seconds
                        );
                        
                        logger.warn(`SignalR reconnect attempt ${retryContext.previousRetryCount + 1}, retrying in ${delay}ms`);
                        return delay;
                    }
                })
                .configureLogging(config.isDevelopment() ? LogLevel.Information : LogLevel.Warning)
                .build();

            // Event handlers
            connection.value.onreconnecting(() => {
                isConnected.value = false;
                reconnectAttempts.value++;
                logger.warn(`SignalR reconnecting... Attempt ${reconnectAttempts.value}`);
                
                if (reconnectAttempts.value === 3) {
                    showNotification({
                        title: 'Connection Lost',
                        message: 'Attempting to reconnect to real-time updates...',
                        type: 'warning'
                    });
                }
            });

            connection.value.onreconnected(() => {
                isConnected.value = true;
                reconnectAttempts.value = 0;
                logger.info('SignalR reconnected successfully');
                
                showNotification({
                    title: 'Connection Restored',
                    message: 'Real-time updates are now active.',
                    type: 'success',
                    timeout: 3000
                });
            });

            connection.value.onclose((error) => {
                isConnected.value = false;
                isConnecting.value = false;
                
                if (error) {
                    logger.error('SignalR connection closed with error:', error);
                    showNotification({
                        title: 'Connection Lost',
                        message: 'Real-time updates are temporarily unavailable.',
                        type: 'error'
                    });
                } else {
                    logger.info('SignalR connection closed normally');
                }
            });

            await connection.value.start();
            isConnected.value = true;
            isConnecting.value = false;
            reconnectAttempts.value = 0;
            
            logger.info('SignalR connected successfully');
            
        } catch (err) {
            isConnecting.value = false;
            const error = err as Error;
            
            logger.error('SignalR connection failed:', {
                error: error.message,
                stack: error.stack
            });
            
            showNotification({
                title: 'Connection Failed',
                message: 'Unable to connect to real-time updates. Some features may be limited.',
                type: 'error'
            });
        }
    };

    const on = (methodName: string, newMethod: (...args: any[]) => void) => {
        if (!connection.value) {
            logger.warn(`Cannot register handler for ${methodName}: connection not established`);
            return;
        }
        
        connection.value.on(methodName, newMethod);
        logger.debug(`SignalR handler registered for: ${methodName}`);
    };

    const off = (methodName: string, newMethod?: (...args: any[]) => void) => {
        if (!connection.value) {
            logger.warn(`Cannot unregister handler for ${methodName}: connection not established`);
            return;
        }
        
        connection.value.off(methodName, newMethod);
        logger.debug(`SignalR handler unregistered for: ${methodName}`);
    };

    const stopConnection = async () => {
        if (!connection.value) {
            return;
        }

        try {
            logger.info('Stopping SignalR connection...');
            await connection.value.stop();
            
            connection.value = null;
            isConnected.value = false;
            isConnecting.value = false;
            reconnectAttempts.value = 0;
            
            logger.info('SignalR connection stopped');
        } catch (err) {
            logger.error('Error stopping SignalR connection:', err);
        }
    };

    const invoke = async (methodName: string, ...args: any[]): Promise<any> => {
        if (!connection.value || !isConnected.value) {
            throw new Error('SignalR connection not established');
        }

        try {
            logger.debug(`Invoking SignalR method: ${methodName}`, { args });
            const result = await connection.value.invoke(methodName, ...args);
            logger.debug(`SignalR method ${methodName} completed successfully`);
            return result;
        } catch (err) {
            logger.error(`Error invoking SignalR method ${methodName}:`, err);
            throw err;
        }
    };

    // Cleanup on component unmount
    onUnmounted(() => {
        if (connection.value) {
            stopConnection();
        }
    });

    return {
        connection: readonly(connection),
        isConnected: readonly(isConnected),
        isConnecting: readonly(isConnecting),
        reconnectAttempts: readonly(reconnectAttempts),
        startConnection,
        stopConnection,
        on,
        off,
        invoke
    };
}
