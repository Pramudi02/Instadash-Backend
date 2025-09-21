# InstaDash Backend - Production Monitoring Guide

## 📊 Application Insights Configuration

Your application is already configured with Application Insights:
- **Connection String**: Automatically configured via environment variables
- **Resource ID**: `/subscriptions/84fd47c7-6b32-4376-b8ed-c323a455703a/resourceGroups/instadash-backend/providers/Microsoft.Insights/components/aiue5te3lwwi77g`

## 🚨 Recommended Alerts

### 1. Application Availability Alert
```bash
# Create availability alert
az monitor metrics alert create \
  --name "InstaDash-Backend-Availability" \
  --resource-group "instadash-backend" \
  --scopes "/subscriptions/84fd47c7-6b32-4376-b8ed-c323a455703a/resourceGroups/instadash-backend/providers/Microsoft.Web/sites/appue5te3lwwi77g" \
  --condition "avg availabilityResults/availabilityPercentage < 95" \
  --description "Alert when availability drops below 95%" \
  --evaluation-frequency 5m \
  --window-size 15m \
  --severity 2
```

### 2. High Response Time Alert
```bash
# Create response time alert
az monitor metrics alert create \
  --name "InstaDash-Backend-ResponseTime" \
  --resource-group "instadash-backend" \
  --scopes "/subscriptions/84fd47c7-6b32-4376-b8ed-c323a455703a/resourceGroups/instadash-backend/providers/Microsoft.Web/sites/appue5te3lwwi77g" \
  --condition "avg requests/duration > 5000" \
  --description "Alert when average response time exceeds 5 seconds" \
  --evaluation-frequency 5m \
  --window-size 15m \
  --severity 3
```

### 3. Exception Rate Alert
```bash
# Create exception alert
az monitor metrics alert create \
  --name "InstaDash-Backend-Exceptions" \
  --resource-group "instadash-backend" \
  --scopes "/subscriptions/84fd47c7-6b32-4376-b8ed-c323a455703a/resourceGroups/instadash-backend/providers/Microsoft.Insights/components/aiue5te3lwwi77g" \
  --condition "count exceptions/count > 10" \
  --description "Alert when exception count exceeds 10 in 15 minutes" \
  --evaluation-frequency 5m \
  --window-size 15m \
  --severity 2
```

## 📈 Key Metrics to Monitor

### Performance Metrics
- **Response Time**: Target < 2 seconds for 95th percentile
- **Throughput**: Requests per second
- **CPU Usage**: Target < 80%
- **Memory Usage**: Target < 80%

### Reliability Metrics  
- **Availability**: Target > 99.9%
- **Error Rate**: Target < 1%
- **Exception Count**: Monitor spikes

### Business Metrics
- **API Endpoint Usage**: Track most used endpoints
- **User Authentication**: Monitor login success/failure rates
- **Database Operations**: Monitor MongoDB connection health

## 🔍 Useful KQL Queries for Application Insights

### Top 10 Slowest Requests
```kusto
requests
| where timestamp > ago(1h)
| top 10 by duration desc
| project timestamp, name, duration, resultCode, success
```

### Exception Analysis
```kusto
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc
```

### Dependency Failures
```kusto
dependencies
| where timestamp > ago(1h)
| where success == false
| summarize count() by name, resultCode
| order by count_ desc
```

### Custom Events (MongoDB Operations)
```kusto
customEvents
| where timestamp > ago(1h)
| where name contains "MongoDB"
| summarize count() by name
| order by count_ desc
```

## 🔧 Health Check Endpoints

Your application includes these diagnostic endpoints:
- `/test-env` - Environment variable check
- `/test-database-connection` - MongoDB connectivity test

Consider adding more health checks:
- `/health` - Overall application health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

## 📱 Notification Channels

Configure these notification methods:
1. **Email**: Add your email to action groups
2. **SMS**: For critical alerts
3. **Slack/Teams**: For team notifications
4. **Webhook**: For custom integrations

## 🔄 Automated Responses

Consider implementing:
1. **Auto-scaling**: Based on CPU/memory thresholds
2. **Circuit Breaker**: For external dependencies
3. **Retry Logic**: For transient failures
4. **Graceful Degradation**: When services are unavailable

## 📊 Dashboard Recommendations

Create Azure Dashboards with:
1. **Overview Dashboard**: Key metrics at a glance
2. **Performance Dashboard**: Response times, throughput
3. **Error Dashboard**: Exceptions, failed requests
4. **Business Dashboard**: User activity, feature usage