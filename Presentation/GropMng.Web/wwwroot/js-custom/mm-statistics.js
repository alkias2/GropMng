(function (window, document) {
    'use strict';

    // ─── Color palette – change these to restyle all charts ───────────────────
    var PALETTE = {
        income: {
            barBackground:   'rgba(40, 199, 111, 0.72)',
            barBorder:       '#28c76f',
            avgLine:         '#0d7a3e',
            avgBackground:   'rgba(13, 122, 62, 0.15)',
            gridColor:       'rgba(40, 199, 111, 0.18)'
        },
        expense: {
            barBackground:   'rgba(234, 84, 85, 0.72)',
            barBorder:       '#ea5455',
            avgLine:         '#50a517',
            avgBackground:   'rgba(165, 23, 23, 0.12)',
            gridColor:       'rgba(234, 84, 85, 0.18)'
        },
        neutral: {
            barBackground:   'rgba(115, 103, 240, 0.72)',
            barBorder:       '#7367f0',
            avgLine:         '#4839eb',
            avgBackground:   'rgba(72, 57, 235, 0.12)',
            gridColor:       'rgba(115, 103, 240, 0.18)'
        },
        axisText:        '#6e6b7b',
        axisGrid:        'rgba(110, 107, 123, 0.22)',
        axisGridStrong:  'rgba(110, 107, 123, 0.45)',
        hoverGuide:      'rgba(244, 243, 255, 0.65)'
    };
    // ──────────────────────────────────────────────────────────────────────────

    function createMonthlyAverageHoverPlugin() {
        return {
            id: 'monthlyAverageHoverGuide',
            afterEvent: function (chart, args) {
                var tooltip = chart.tooltip;
                var previousIndex = chart.$avgHoverIndex;
                var nextIndex = null;
                var hasTooltipPoints = !!(tooltip && tooltip.dataPoints && tooltip.dataPoints.length);

                if (tooltip && tooltip.dataPoints && tooltip.dataPoints.length) {
                    var avgPoint = tooltip.dataPoints.find(function (point) {
                        return point.dataset && point.dataset.label === 'Μηνιαίος Μ.Ο.' && Number.isFinite(point.raw);
                    });

                    if (avgPoint) {
                        nextIndex = avgPoint.dataIndex;
                    } else {
                        // Fallback: if tooltip is on a month tick from another dataset, still highlight that x label.
                        var fallbackPoint = tooltip.dataPoints[0];
                        var fallbackIndex = fallbackPoint ? fallbackPoint.dataIndex : null;

                        if (Number.isInteger(fallbackIndex) && chart.data && chart.data.labels && chart.data.labels[fallbackIndex]) {
                            nextIndex = fallbackIndex;
                        }
                    }
                }

                chart.$avgHoverIndex = nextIndex;

                if (previousIndex !== nextIndex || hasTooltipPoints) {
                    if (args) {
                        // Tell Chart.js this event changed visual state so it must re-render ticks/guide.
                        args.changed = true;
                    } else {
                        chart.update('none');
                    }

                    // Some Chart.js category-axis tick styles stay cached until resize/zoom.
                    // Force one no-animation update in the next frame to refresh tick rendering.
                    if (!chart.$mmHoverRefreshScheduled) {
                        chart.$mmHoverRefreshScheduled = true;
                        window.requestAnimationFrame(function () {
                            chart.$mmHoverRefreshScheduled = false;
                            chart.update('none');
                        });
                    }
                }
            },
            afterDatasetsDraw: function (chart) {
                var hoverIndex = chart.$avgHoverIndex;
                if (!Number.isInteger(hoverIndex)) {
                    return;
                }

                var xScale = chart.scales.x;
                var chartArea = chart.chartArea;
                if (!xScale || !chartArea) {
                    return;
                }

                var x = xScale.getPixelForValue(hoverIndex);
                var ctx = chart.ctx;

                ctx.save();
                ctx.setLineDash([4, 4]);
                ctx.lineWidth = 1;
                ctx.strokeStyle = PALETTE.hoverGuide;
                ctx.beginPath();
                ctx.moveTo(x, chartArea.top);
                ctx.lineTo(x, chartArea.bottom);
                ctx.stroke();
                ctx.restore();
            }
        };
    }

    function initializeCategoryStatisticsChart() {
        var canvas = document.getElementById('mmCategoryStatisticsChart');
        var payloadElement = document.getElementById('mmCategoryStatisticsChartData');

        if (!canvas || !payloadElement || typeof window.Chart !== 'function') {
            return;
        }

        var payload;
        try {
            payload = JSON.parse(payloadElement.textContent || '{}');
        } catch (error) {
            window.console.error('Unable to parse chart payload:', error);
            return;
        }

        var labels = Array.isArray(payload.labels) ? payload.labels : [];
        var values = Array.isArray(payload.values)
            ? payload.values.map(function (v) {
                var n = Number(v);
                return Number.isFinite(n) ? n : 0;
            })
            : [];
        var monthKeys = Array.isArray(payload.monthKeys) ? payload.monthKeys : [];

        // Pick colour set based on categoryType: 0 = income, 1 = expense
        var colors = payload.categoryType === 0 ? PALETTE.income : PALETTE.expense;

        // Monthly average line: monthly values only on month ticks, smooth line between months.
        var monthlyTotals = {};
        var monthlyCounts = {};
        var monthTickIndexes = [];

        labels.forEach(function (label, idx) {
            if (label && String(label).trim().length > 0) {
                monthTickIndexes.push(idx);
            }
        });

        monthKeys.forEach(function (monthKey, index) {
            if (!monthKey) {
                return;
            }

            monthlyTotals[monthKey] = (monthlyTotals[monthKey] || 0) + (values[index] || 0);
            monthlyCounts[monthKey] = (monthlyCounts[monthKey] || 0) + 1;
        });

        var avgData = monthKeys.map(function (monthKey, index) {
            var isMonthTick = monthTickIndexes.indexOf(index) >= 0;
            if (!isMonthTick || !monthKey || !monthlyCounts[monthKey]) {
                return null;
            }

            return monthlyTotals[monthKey] / monthlyCounts[monthKey];
        });

        var avgDataNumeric = avgData.filter(function (v) { return Number.isFinite(v); });
        var maxAvgValue = avgDataNumeric.length ? Math.max.apply(Math, avgDataNumeric) : 0;

        var maxValue    = values.length ? Math.max.apply(Math, values) : 0;
        var suggestedMax = Math.max(10, Math.ceil(Math.max(maxValue, maxAvgValue) / 10) * 10);

        var monthlyAverageHoverPlugin = createMonthlyAverageHoverPlugin();

        new window.Chart(canvas, {
            type: 'bar',
            plugins: [monthlyAverageHoverPlugin],
            data: {
                labels: labels,
                datasets: [
                    {
                        type: 'bar',
                        label: 'Ποσό',
                        data: values,
                        backgroundColor: colors.barBackground,
                        borderColor:     colors.barBorder,
                        borderWidth:     1,
                        borderRadius:    3,
                        order: 2
                    },
                    {
                        type: 'line',
                        label: 'Μηνιαίος Μ.Ο.',
                        data: avgData,
                        borderColor:     colors.avgLine,
                        backgroundColor: colors.avgBackground,
                        borderWidth:     3,
                        pointRadius:     0,
                        pointHoverRadius: 0,
                        pointHitRadius:   18,
                        spanGaps:        true,
                        fill:            false,
                        tension:         0.35,
                        order: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 350 },
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            color: PALETTE.axisText,
                            usePointStyle: true,
                            pointStyle: 'circle',
                            boxWidth: 10,
                            padding: 16
                        }
                    },
                    tooltip: {
                        filter: function (ctx) {
                            // For monthly average, show tooltip only on month ticks.
                            if (ctx.dataset && ctx.dataset.label === 'Μηνιαίος Μ.Ο.') {
                                return Number.isFinite(ctx.raw);
                            }

                            return true;
                        },
                        callbacks: {
                            label: function (ctx) {
                                return ' ' + ctx.dataset.label + ': ' + Number(ctx.raw).toFixed(2) + ' €';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: true,
                            color: PALETTE.axisGrid,
                            lineWidth: 1
                        },
                        border: { display: false },
                        ticks: {
                            autoSkip: false,
                            minRotation: 90,
                            maxRotation: 90,
                            color: function (ctx) {
                                var hoverIndex = ctx.chart.$avgHoverIndex;
                                var hoverLabel = Number.isInteger(hoverIndex) && labels[hoverIndex]
                                    ? String(labels[hoverIndex]).trim()
                                    : '';
                                var tickLabel = '';

                                if (ctx.tick && ctx.tick.label != null) {
                                    tickLabel = String(ctx.tick.label).trim();
                                } else if (Number.isInteger(ctx.index) && labels[ctx.index]) {
                                    tickLabel = String(labels[ctx.index]).trim();
                                } else if (ctx.tick && ctx.chart && ctx.chart.scales && ctx.chart.scales.x) {
                                    tickLabel = String(ctx.chart.scales.x.getLabelForValue(ctx.tick.value) || '').trim();
                                }

                                if (hoverLabel && tickLabel && hoverLabel === tickLabel) {
                                    return colors.avgLine;
                                }

                                return PALETTE.axisText;
                            },
                            font: function (ctx) {
                                var hoverIndex = ctx.chart.$avgHoverIndex;
                                var hoverLabel = Number.isInteger(hoverIndex) && labels[hoverIndex]
                                    ? String(labels[hoverIndex]).trim()
                                    : '';
                                var tickLabel = '';

                                if (ctx.tick && ctx.tick.label != null) {
                                    tickLabel = String(ctx.tick.label).trim();
                                } else if (Number.isInteger(ctx.index) && labels[ctx.index]) {
                                    tickLabel = String(labels[ctx.index]).trim();
                                } else if (ctx.tick && ctx.chart && ctx.chart.scales && ctx.chart.scales.x) {
                                    tickLabel = String(ctx.chart.scales.x.getLabelForValue(ctx.tick.value) || '').trim();
                                }

                                var isActive = hoverLabel && tickLabel && hoverLabel === tickLabel;

                                return {
                                    size: 11,
                                    weight: isActive ? '700' : '400'
                                };
                            },
                            callback: function (value, index) {
                                return labels[index] || '';
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        suggestedMax: suggestedMax,
                        grid: {
                            display: true,
                            color: function (ctx) {
                                // Emphasise every 5th gridline for readability
                                return ctx.tick.value % 50 === 0
                                    ? PALETTE.axisGridStrong
                                    : PALETTE.axisGrid;
                            },
                            lineWidth: function (ctx) {
                                return ctx.tick.value % 50 === 0 ? 1.5 : 1;
                            }
                        },
                        border: { display: false },
                        ticks: {
                            stepSize: 10,
                            color: PALETTE.axisText,
                            font: { size: 11 },
                            callback: function (value) {
                                return Number(value).toFixed(0);
                            }
                        }
                    }
                }
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCategoryStatisticsChart);
    } else {
        initializeCategoryStatisticsChart();
    }
})(window, document);
